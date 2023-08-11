using System.Collections.Concurrent;
using System.Collections.Immutable;
using AyBorg.Agent.Runtime;
using AyBorg.Agent.Services;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Result;

namespace AyBorg.Agent.Result;

public class ResultStorageProvider : IResultStorageProvider, IDisposable
{
    private readonly ILogger<ResultStorageProvider> _logger;
    private readonly IEngineHost _engineHost;
    private readonly ConcurrentQueue<WorkflowResult> _finishedWorkflowResults = new();
    private ImmutableHashSet<WorkflowResult> _runningWorkflowResults = ImmutableHashSet<WorkflowResult>.Empty;
    private readonly Task _executionTask;
    private readonly CancellationTokenSource _executionCancellationTokenSource = new();
    private Guid _currentIterationId = Guid.Empty;
    private bool _isDisposed = false;


    public ResultStorageProvider(ILogger<ResultStorageProvider> logger, IEngineHost engineHost)
    {
        _logger = logger;
        _engineHost = engineHost;
        _engineHost.IterationStarted += OnIterationStarted;
        _engineHost.IterationFinished += OnIterationFinished;
        _executionTask = Task.Factory.StartNew(() => ExecutionLoop(_executionCancellationTokenSource.Token), TaskCreationOptions.LongRunning);
    }

    public void Add(PortResult result)
    {
        bool isNewWorkflow = false;
        WorkflowResult? activeWorkflowResult = _runningWorkflowResults.FirstOrDefault(wr => wr.IterationId.Equals(_currentIterationId));

        if (activeWorkflowResult == null)
        {
            isNewWorkflow = true;
            activeWorkflowResult = new WorkflowResult
            {
                IterationId = _currentIterationId,
                StartTime = DateTime.UtcNow
            };
        }

        if (activeWorkflowResult.PortResults.Any(pr => pr.Id.Equals(result.Id, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new InvalidOperationException($"PortResult with id '{result.Id}' already exists.");
        }

        activeWorkflowResult.PortResults.Add(result);
        if (isNewWorkflow)
        {
            _runningWorkflowResults = _runningWorkflowResults.Add(activeWorkflowResult);
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
                _logger.LogInformation(new EventId((int)EventLogType.Result), "Workflow result for iteration '{iterationId}' processing...", finishedResult.IterationId);
            }
        }
    }

    private void OnIterationStarted(object? sender, IterationStartedEventArgs e)
    {
        _currentIterationId = e.IterationId;
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
        _runningWorkflowResults = _runningWorkflowResults.Remove(finishedWorkflow);
        _finishedWorkflowResults.Enqueue(finishedWorkflow);
    }
}
