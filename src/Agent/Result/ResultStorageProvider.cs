using System.Collections.Concurrent;
using System.Collections.Immutable;
using Ayborg.Gateway.Result.V1;
using AyBorg.Agent.Runtime;
using AyBorg.Agent.Services;
using AyBorg.Agent.Services.gRPC;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Result;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.System.Configuration;
using ImageTorque;

namespace AyBorg.Agent.Result;

public class ResultStorageProvider : IResultStorageProvider, IDisposable
{
    private readonly ILogger<ResultStorageProvider> _logger;
    private readonly IServiceConfiguration _serviceConfiguration;
    private readonly IEngineHost _engineHost;
    private readonly IRpcMapper _rpcMapper;
    private readonly ConcurrentQueue<WorkflowResult> _finishedWorkflowResults = new();
    private readonly Storage.StorageClient _storageClient;
    private ImmutableHashSet<WorkflowResult> _runningWorkflowResults = ImmutableHashSet<WorkflowResult>.Empty;
    private readonly Task _executionTask;
    private readonly CancellationTokenSource _executionCancellationTokenSource = new();
    private Guid _currentIterationId = Guid.Empty;
    private bool _isDisposed = false;

    public ResultStorageProvider(ILogger<ResultStorageProvider> logger,
                                    IServiceConfiguration serviceConfiguration,
                                    IEngineHost engineHost,
                                    IRpcMapper rpcMapper,
                                    Storage.StorageClient storageClient)
    {
        _logger = logger;
        _serviceConfiguration = serviceConfiguration;
        _engineHost = engineHost;
        _rpcMapper = rpcMapper;
        _storageClient = storageClient;
        _engineHost.IterationStarted += OnIterationStarted;
        _engineHost.IterationFinished += OnIterationFinished;
        _executionTask = Task.Factory.StartNew(() => ExecutionLoop(_executionCancellationTokenSource.Token), TaskCreationOptions.LongRunning);
    }

    public void Add(PortResult result)
    {
        WorkflowResult? activeWorkflowResult = _runningWorkflowResults.First(wr => wr.IterationId.Equals(_currentIterationId));

        if (activeWorkflowResult.PortResults.Any(pr => pr.Id.Equals(result.Id, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new InvalidOperationException($"PortResult with id '{result.Id}' already exists.");
        }

        if (!activeWorkflowResult.PortResults.TryAdd(result))
        {
            throw new InvalidOperationException($"Failed to add PortResult with id '{result.Id}'.");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            _engineHost.IterationStarted -= OnIterationStarted;
            _engineHost.IterationFinished -= OnIterationFinished;
            _executionCancellationTokenSource.Cancel();
            _executionTask?.Wait();
            _executionTask?.Dispose();
            _isDisposed = true;
        }
    }

    private async void ExecutionLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_finishedWorkflowResults.IsEmpty)
            {
                await Task.Delay(10, CancellationToken.None);
            }
            else if (_finishedWorkflowResults.TryDequeue(out WorkflowResult? finishedResult))
            {
                try
                {
                    string resultId = finishedResult.Id.ToString();
                    string iterationId = finishedResult.IterationId.ToString();

                    var request = new AddRequest
                    {
                        Id = resultId,
                        AgentUniqueName = _serviceConfiguration.UniqueName,
                        IterationId = iterationId,
                        StartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(finishedResult.StartTime),
                        StopTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(finishedResult.StopTime),
                        ElapsedMs = finishedResult.ElapsedMs,
                        Success = finishedResult.Success
                    };

                    foreach (PortResult portResult in finishedResult.PortResults)
                    {
                        Ayborg.Gateway.Agent.V1.PortDto tmpRpcPort = _rpcMapper.ToRpc(portResult.Port);
                        var rpcPort = new PortDto
                        {
                            Id = tmpRpcPort.Id,
                            Name = portResult.Id,
                            Direction = tmpRpcPort.Direction,
                            Brand = tmpRpcPort.Brand,
                            Value = tmpRpcPort.Value
                        };
                        request.Ports.Add(rpcPort);

                        // If image, send as chunks
                        if (portResult.Port.Brand == SDK.Common.Ports.PortBrand.Image)
                        {
                            var cacheImage = (CacheImage)portResult.Port.Value!;
                            using Grpc.Core.AsyncClientStreamingCall<ImageChunkDto, Google.Protobuf.WellKnownTypes.Empty> imageStreamCall = _storageClient.AddImage(cancellationToken: cancellationToken);
                            await ImageStreamer.SendImageAsync((Image)cacheImage.OriginalImage!, imageStreamCall.RequestStream, iterationId, portResult.Id, portResult.ScaleFactor, cancellationToken);
                            await imageStreamCall;
                        }
                    }

                    await _storageClient.AddAsync(request, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId((int)EventLogType.Result), ex, "Error while storing result.");
                }
            }
        }
    }

    private void OnIterationStarted(object? sender, IterationStartedEventArgs e)
    {
        _currentIterationId = e.IterationId;
        WorkflowResult? activeWorkflowResult = _runningWorkflowResults.FirstOrDefault(wr => wr.IterationId.Equals(_currentIterationId));
        if (activeWorkflowResult == null)
        {
            activeWorkflowResult = new WorkflowResult
            {
                IterationId = _currentIterationId,
                StartTime = DateTime.UtcNow
            };

            _runningWorkflowResults = _runningWorkflowResults.Add(activeWorkflowResult);
        }
    }

    private void OnIterationFinished(object? sender, IterationFinishedEventArgs e)
    {
        WorkflowResult? finishedWorkflow = _runningWorkflowResults.FirstOrDefault(w => w.IterationId.Equals(e.IterationId));
        if (finishedWorkflow == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Result), "No workflow result found for iteration '{iterationId}'.", e.IterationId);
            return;
        }

        finishedWorkflow.StopTime = DateTime.UtcNow;
        finishedWorkflow.ElapsedMs = (int)(finishedWorkflow.StopTime - finishedWorkflow.StartTime).TotalMilliseconds;
        finishedWorkflow.Success = e.Success;
        _runningWorkflowResults = _runningWorkflowResults.Remove(finishedWorkflow);
        _finishedWorkflowResults.Enqueue(finishedWorkflow);
    }
}
