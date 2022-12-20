using System.Buffers;
using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.ImageProcessing;
using AyBorg.SDK.System.Agent;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.IO;

namespace AyBorg.Agent.Services.gRPC;

public sealed class EditorServiceV1 : Editor.EditorBase
{
    private static readonly RecyclableMemoryStreamManager s_memoryManager = new();
    private readonly ILogger<EditorServiceV1> _logger;
    private readonly IPluginsService _pluginsService;
    private readonly IFlowService _flowService;
    private readonly ICacheService _cacheService;

    public EditorServiceV1(ILogger<EditorServiceV1> logger,
                            IPluginsService pluginsService,
                            IFlowService flowService,
                            ICacheService cacheService)
    {
        _logger = logger;
        _pluginsService = pluginsService;
        _flowService = flowService;
        _cacheService = cacheService;
    }

    public override async Task<GetAvailableStepsResponse> GetAvailableSteps(GetAvailableStepsRequest request, ServerCallContext context)
    {
        var result = new GetAvailableStepsResponse();
        foreach (SDK.Common.IStepProxy step in _pluginsService.Steps)
        {
            Step stepBinding = await RuntimeMapper.FromRuntimeAsync(step);
            StepDto rpcStep = RpcMapper.ToRpc(stepBinding);
            result.Steps.Add(rpcStep);
        }

        return result;
    }

    public override async Task<GetFlowStepsResponse> GetFlowSteps(GetFlowStepsRequest request, ServerCallContext context)
    {
        var result = new GetFlowStepsResponse();

        IEnumerable<SDK.Common.IStepProxy> flowSteps = _flowService.GetSteps();
        foreach (SDK.Common.IStepProxy fs in flowSteps)
        {
            result.Steps.Add(RpcMapper.ToRpc(await RuntimeMapper.FromRuntimeAsync(fs)));
        }

        return result;
    }

    public override Task<GetFlowLinksResponse> GetFlowLinks(GetFlowLinksRequest request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            var result = new GetFlowLinksResponse();

            IEnumerable<PortLink> flowLinks = _flowService.GetLinks();
            foreach (PortLink fl in flowLinks)
            {
                result.Links.Add(RpcMapper.ToRpc(fl));
            }
            return result;
        });
    }

    public override async Task<GetFlowPortsResponse> GetFlowPorts(GetFlowPortsRequest request, ServerCallContext context)
    {
        var resultPorts = new List<PortDto>();
        Guid iterationId = Guid.Empty;
        if (!string.IsNullOrEmpty(request.IterationId))
        {
            if (!Guid.TryParse(request.IterationId, out iterationId))
            {
                _logger.LogWarning("Invalid iteration id: {IterationId}", request.IterationId);
            }
        }

        foreach (string? portIdStr in request.PortIds)
        {
            if (!Guid.TryParse(portIdStr, out Guid portId))
            {
                _logger.LogWarning("Invalid port id: {PortId}", portIdStr);
                continue;
            }

            IPort port = _flowService.GetPort(portId);
            if (port == null)
            {
                _logger.LogWarning("Port not found: {PortId}", portId);
                continue;
            }

            if (iterationId != Guid.Empty)
            {
                resultPorts.Add(RpcMapper.ToRpc(await _cacheService.GetOrCreatePortEntryAsync(iterationId, port)));
            }
            else
            {
                resultPorts.Add(RpcMapper.ToRpc(await RuntimeMapper.FromRuntimeAsync(port)));
            }
        }

        var result = new GetFlowPortsResponse();
        result.Ports.Add(resultPorts);
        return await ValueTask.FromResult(result);
    }

    public override async Task<AddFlowStepResponse> AddFlowStep(AddFlowStepRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.StepId, out Guid stepId))
        {
            _logger.LogWarning("Invalid step id: {StepId}", request.StepId);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid step id"));
        }

        SDK.Common.IStepProxy stepProxy = await _flowService.AddStepAsync(stepId, request.X, request.Y);
        if (stepProxy == null)
        {
            _logger.LogWarning("Step not found: {StepId}", request.StepId);
            throw new RpcException(new Status(StatusCode.NotFound, "Step not found"));
        }

        return new AddFlowStepResponse
        {
            Step = RpcMapper.ToRpc(await RuntimeMapper.FromRuntimeAsync(stepProxy))
        };
    }

    public override async Task<Empty> DeleteFlowStep(DeleteFlowStepRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.StepId, out Guid stepId))
        {
            _logger.LogWarning("Invalid step id: {StepId}", request.StepId);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid step id"));
        }

        if (!await _flowService.TryRemoveStepAsync(stepId))
        {
            _logger.LogWarning("Step not found: {StepId}", request.StepId);
            throw new RpcException(new Status(StatusCode.NotFound, "Step not found"));
        }

        return new Empty();
    }

    public override async Task<Empty> MoveFlowStep(MoveFlowStepRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.StepId, out Guid stepId))
        {
            _logger.LogWarning("Invalid step id: {StepId}", request.StepId);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid step id"));
        }

        if (!await _flowService.TryMoveStepAsync(stepId, request.X, request.Y))
        {
            _logger.LogWarning("Step not found: {StepId}", request.StepId);
            throw new RpcException(new Status(StatusCode.NotFound, "Step not found"));
        }

        return new Empty();
    }

    public override async Task<Empty> LinkFlowPorts(LinkFlowPortsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.SourceId, out Guid sourceId))
        {
            _logger.LogWarning("Invalid source id: {SourceId}", request.SourceId);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid source id"));
        }

        if (!string.IsNullOrEmpty(request.TargetId))
        {
            // Try to link
            if (!Guid.TryParse(request.TargetId, out Guid targetId))
            {
                _logger.LogWarning("Invalid target id: {TargetId}", request.TargetId);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid target id"));
            }

            _ = await _flowService.LinkPortsAsync(sourceId, targetId);
        }
        else
        {
            // Try to unlink
            if (!await _flowService.TryUnlinkPortsAsync(sourceId))
            {
                _logger.LogWarning("Source port not found: {SourceId}", request.SourceId);
                throw new RpcException(new Status(StatusCode.NotFound, "Source port not found"));
            }
        }

        return new Empty();
    }

    public override async Task<Empty> UpdateFlowPort(UpdateFlowPortRequest request, ServerCallContext context)
    {
        Port port = RpcMapper.FromRpc(request.Port);
        if (!await _flowService.TryUpdatePortValueAsync(port.Id, port.Value!))
        {
            _logger.LogWarning("Could not update port: {PortId}", port.Id);
            throw new RpcException(new Status(StatusCode.Internal, "Could not update port"));
        }

        return new Empty();
    }

    public override async Task GetImageStream(GetImageStreamRequest request, IServerStreamWriter<ImageChunkDto> responseStream, ServerCallContext context)
    {
        Guid iterationId = Guid.Empty;
        if (!string.IsNullOrEmpty(request.IterationId))
        {
            if (!Guid.TryParse(request.IterationId, out iterationId))
            {
                _logger.LogWarning("Invalid iteration id: {IterationId}", request.IterationId);
            }
        }
        if (!Guid.TryParse(request.PortId, out Guid portId))
        {
            _logger.LogWarning("Invalid port id: {PortId}", request.PortId);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid port id"));
        }

        IPort port = _flowService.GetPort(portId);
        if (port == null)
        {
            _logger.LogWarning("Port not found: {PortId}", portId);
            throw new RpcException(new Status(StatusCode.NotFound, "Port not found"));
        }

        Port portModel;
        bool isOriginalPortModelCreated = false;
        if (iterationId != Guid.Empty)
        {
            portModel = await _cacheService.GetOrCreatePortEntryAsync(iterationId, port);
        }
        else
        {
            portModel = await RuntimeMapper.FromRuntimeAsync(port);
            isOriginalPortModelCreated = true;
        }

        var originalImageModel = (CacheImage)portModel.Value!;
        Image originalImage = (Image)originalImageModel.OriginalImage!;
        if (originalImage == null)
        {
            _logger.LogTrace("Image not found: {PortId}", portId);
            return;
        }

        const int chunkSize = 32768;
        const int maxSize = 250;
        IImage targetImage = null!;
        try
        {
            if (originalImage.Width <= maxSize && originalImage.Height <= maxSize || !request.AsThumbnail)
            {
                targetImage = originalImage;
            }
            else
            {
                Image.CalculateClampSize(originalImage, maxSize, out int w, out int h);
                targetImage = originalImage.Resize(w, h, ResizeMode.NearestNeighbor);
            }

            using MemoryStream stream = s_memoryManager.GetStream();
            Image.Save(targetImage, stream, request.AsThumbnail ? SDK.ImageProcessing.Encoding.EncoderType.Jpeg : SDK.ImageProcessing.Encoding.EncoderType.Png);
            stream.Position = 0;
            long fullStreamLength = stream.Length;
            long bytesToSend = fullStreamLength;
            int bufferSize = fullStreamLength < chunkSize ? (int)fullStreamLength : chunkSize;
            int offset = 0;
            using IMemoryOwner<byte> memoryOwner = MemoryPool<byte>.Shared.Rent((int)fullStreamLength);
            await stream.ReadAsync(memoryOwner.Memory, context.CancellationToken);

            while (!context.CancellationToken.IsCancellationRequested && bytesToSend > 0)
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
                    FullWidth = targetImage.Width,
                    FullHeight = targetImage.Height,
                    FullStreamLength = fullStreamLength
                });
            }
        }
        finally
        {
            if (isOriginalPortModelCreated)
            {
                portModel.Dispose();
            }
            targetImage?.Dispose();
        }
    }
}