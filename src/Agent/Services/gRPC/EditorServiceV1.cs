using System.Buffers;
using System.Runtime.CompilerServices;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Data.Mapper;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.Projects;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ImageTorque;
using Microsoft.IO;

namespace AyBorg.Agent.Services.gRPC;

public sealed class EditorServiceV1 : Editor.EditorBase
{
    private static readonly RecyclableMemoryStreamManager s_memoryManager = new();
    private readonly ILogger<EditorServiceV1> _logger;
    private readonly IPluginsService _pluginsService;
    private readonly IFlowService _flowService;
    private readonly ICacheService _cacheService;
    private readonly IRuntimeMapper _runtimeMapper;
    private readonly IRpcMapper _rpcMapper;

    public EditorServiceV1(ILogger<EditorServiceV1> logger,
                            IPluginsService pluginsService,
                            IFlowService flowService,
                            ICacheService cacheService,
                            IRuntimeMapper runtimeMapper,
                            IRpcMapper rpcMapper)
    {
        _logger = logger;
        _pluginsService = pluginsService;
        _flowService = flowService;
        _cacheService = cacheService;
        _runtimeMapper = runtimeMapper;
        _rpcMapper = rpcMapper;
    }

    public override Task<GetAvailableStepsResponse> GetAvailableSteps(GetAvailableStepsRequest request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            var result = new GetAvailableStepsResponse();
            foreach (IStepProxy step in _pluginsService.Steps)
            {
                Step stepBinding = _runtimeMapper.FromRuntime(step);
                StepDto rpcStep = _rpcMapper.ToRpc(stepBinding);
                result.Steps.Add(rpcStep);
            }

            return result;
        });
    }

    public override Task<GetFlowStepsResponse> GetFlowSteps(GetFlowStepsRequest request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            var result = new GetFlowStepsResponse();
            Guid iterationId = Guid.Empty;
            if (!string.IsNullOrEmpty(request.IterationId) && !Guid.TryParse(request.IterationId, out iterationId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid iteration id"));
            }

            IEnumerable<IStepProxy> flowSteps = _flowService.GetSteps();
            if (request.StepIds.Any())
            {
                var targetIds = new HashSet<Guid>();
                foreach (string? idStr in request.StepIds)
                {
                    if (!Guid.TryParse(idStr, out Guid stepId))
                    {
                        _logger.LogWarning(new EventId((int)EventLogType.Engine), "Invalid step id: {StepId}", idStr);
                        continue;
                    }

                    targetIds.Add(stepId);
                }

                // Filter steps
                flowSteps = flowSteps.Where(x => targetIds.Contains(x.Id));
            }

            foreach (IStepProxy fs in flowSteps)
            {
                if (iterationId != Guid.Empty)
                {
                    result.Steps.Add(_rpcMapper.ToRpc(_cacheService.GetOrCreateStepEntry(iterationId, fs)));
                }
                else
                {
                    result.Steps.Add(_rpcMapper.ToRpc(_runtimeMapper.FromRuntime(fs)));
                }
            }

            return result;
        });
    }

    public override Task<GetFlowLinksResponse> GetFlowLinks(GetFlowLinksRequest request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            var result = new GetFlowLinksResponse();

            IEnumerable<PortLink> flowLinks = _flowService.GetLinks();
            var targetPorts = new HashSet<PortLink>();
            if (request.LinkIds.Any())
            {
                // Filter links
                foreach (string? idStr in request.LinkIds)
                {
                    if (!Guid.TryParse(idStr, out Guid linkId))
                    {
                        _logger.LogWarning(new EventId((int)EventLogType.Engine), "Invalid link id: {LinkId}", idStr);
                        continue;
                    }

                    targetPorts.Add(flowLinks.First(x => x.Id == linkId));
                }

                flowLinks = targetPorts;
            }

            foreach (PortLink fl in flowLinks)
            {
                result.Links.Add(_rpcMapper.ToRpc(fl));
            }
            return result;
        });
    }

    public override async Task<GetFlowPortsResponse> GetFlowPorts(GetFlowPortsRequest request, ServerCallContext context)
    {
        var resultPorts = new List<PortDto>();
        Guid iterationId = Guid.Empty;
        if (!string.IsNullOrEmpty(request.IterationId) && !Guid.TryParse(request.IterationId, out iterationId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid iteration id"));
        }

        foreach (string? portIdStr in request.PortIds)
        {
            if (!Guid.TryParse(portIdStr, out Guid portId))
            {
                _logger.LogWarning(new EventId((int)EventLogType.Engine), "Invalid port id: {PortId}", portIdStr);
                continue;
            }

            IPort port = _flowService.GetPort(portId);
            if (port == null)
            {
                _logger.LogTrace(new EventId((int)EventLogType.Engine), "Port not found: {PortId}", portId);
                continue;
            }

            if (iterationId != Guid.Empty)
            {
                resultPorts.Add(_rpcMapper.ToRpc(_cacheService.GetOrCreatePortEntry(iterationId, port)));
            }
            else
            {
                resultPorts.Add(_rpcMapper.ToRpc(_runtimeMapper.FromRuntime(port)));
            }
        }

        var result = new GetFlowPortsResponse();
        result.Ports.Add(resultPorts);
        return await ValueTask.FromResult(result);
    }

    public override async Task<AddFlowStepResponse> AddFlowStep(AddFlowStepRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        if (!Guid.TryParse(request.StepId, out Guid stepId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid step id"));
        }

        IStepProxy stepProxy = await _flowService.AddStepAsync(stepId, request.X, request.Y);
        if (stepProxy == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Step not found"));
        }

        return new AddFlowStepResponse
        {
            Step = _rpcMapper.ToRpc(_runtimeMapper.FromRuntime(stepProxy))
        };
    }

    public override async Task<Empty> DeleteFlowStep(DeleteFlowStepRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        if (!Guid.TryParse(request.StepId, out Guid stepId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid step id"));
        }

        if (!await _flowService.TryRemoveStepAsync(stepId))
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Step not found"));
        }

        return new Empty();
    }

    public override async Task<Empty> MoveFlowStep(MoveFlowStepRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        if (!Guid.TryParse(request.StepId, out Guid stepId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid step id"));
        }

        if (!await _flowService.TryMoveStepAsync(stepId, request.X, request.Y))
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Step not found"));
        }

        return new Empty();
    }

    public override async Task<LinkFlowPortsResponse> LinkFlowPorts(LinkFlowPortsRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        if (!Guid.TryParse(request.SourceId, out Guid sourceId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid source id"));
        }

        if (!string.IsNullOrEmpty(request.TargetId))
        {
            // Try to link
            if (!Guid.TryParse(request.TargetId, out Guid targetId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid target id"));
            }

            PortLink newPortLink = await _flowService.LinkPortsAsync(sourceId, targetId);
            if(newPortLink == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Invalid link"));
            }
            return new LinkFlowPortsResponse
            {
                LinkId = newPortLink.Id.ToString()
            };
        }
        else
        {
            // Try to unlink
            if (!await _flowService.TryUnlinkPortsAsync(sourceId))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Source port not found"));
            }

            return new LinkFlowPortsResponse { LinkId = string.Empty };
        }
    }

    public override async Task<Empty> UpdateFlowPort(UpdateFlowPortRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        Port port = _rpcMapper.FromRpc(request.Port);
        if (!await _flowService.TryUpdatePortValueAsync(port.Id, port.Value!))
        {
            throw new RpcException(new Status(StatusCode.Internal, "Could not update port"));
        }

        return new Empty();
    }

    public override async Task GetImageStream(GetImageStreamRequest request, IServerStreamWriter<ImageChunkDto> responseStream, ServerCallContext context)
    {
        Guid iterationId = Guid.Empty;
        if (!string.IsNullOrEmpty(request.IterationId) && !Guid.TryParse(request.IterationId, out iterationId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid iteration id"));
        }

        if (!Guid.TryParse(request.PortId, out Guid portId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid port id"));
        }

        IPort port = _flowService.GetPort(portId) ?? throw new RpcException(new Status(StatusCode.NotFound, "Port not found"));
        Port portModel;
        bool isOriginalPortModelCreated = false;
        if (iterationId != Guid.Empty)
        {
            portModel = _cacheService.GetOrCreatePortEntry(iterationId, port);
        }
        else
        {
            portModel = _runtimeMapper.FromRuntime(port);
            isOriginalPortModelCreated = true;
        }

        var originalImageModel = (CacheImage)portModel.Value!;
        Image originalImage = (Image)originalImageModel.OriginalImage!;
        if (originalImage == null)
        {
            _logger.LogTrace(new EventId((int)EventLogType.Engine), "Image not found: {PortId}", portId);
            return;
        }

        await SendImageAsync(originalImage, responseStream, request.AsThumbnail, context.CancellationToken);

        if (isOriginalPortModelCreated)
        {
            portModel.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask SendImageAsync(Image originalImage, IServerStreamWriter<ImageChunkDto> responseStream, bool asThumbnail, CancellationToken cancellationToken)
    {
        const int chunkSize = 32768;
        const int maxSize = 250;
        IImage targetImage = null!;
        try
        {
            if (originalImage.Width <= maxSize && originalImage.Height <= maxSize || !asThumbnail)
            {
                targetImage = originalImage;
            }
            else
            {
                originalImage.CalculateClampSize(maxSize, out int w, out int h);
                targetImage = originalImage.Resize(w, h, ResizeMode.NearestNeighbor);
            }

            using MemoryStream stream = s_memoryManager.GetStream();
            targetImage.Save(stream, ImageTorque.Processing.EncoderType.Jpeg);
            stream.Position = 0;
            long fullStreamLength = stream.Length;
            long bytesToSend = fullStreamLength;
            int bufferSize = fullStreamLength < chunkSize ? (int)fullStreamLength : chunkSize;
            int offset = 0;
            using IMemoryOwner<byte> memoryOwner = MemoryPool<byte>.Shared.Rent((int)fullStreamLength);
            await stream.ReadAsync(memoryOwner.Memory, cancellationToken);

            while (!cancellationToken.IsCancellationRequested && bytesToSend > 0)
            {
                if (bytesToSend < bufferSize)
                {
                    bufferSize = (int)bytesToSend;
                }

                Memory<byte> slice = memoryOwner.Memory.Slice(offset, bufferSize);

                bytesToSend -= bufferSize;
                offset += bufferSize;

                await responseStream.WriteAsync(new ImageChunkDto
                {
                    Data = UnsafeByteOperations.UnsafeWrap(slice),
                    FullWidth = originalImage.Width,
                    FullHeight = originalImage.Height,
                    FullStreamLength = fullStreamLength,
                    ScaledWidth = targetImage.Width,
                    ScaledHeight = targetImage.Height
                }, cancellationToken);
            }
        }
        finally
        {
            targetImage?.Dispose();
        }
    }
}
