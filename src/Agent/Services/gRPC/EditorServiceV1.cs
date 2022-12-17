using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.System.Agent;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class EditorServiceV1 : AgentEditor.AgentEditorBase
{
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

    public override Task<GetAvailableStepsResponse> GetAvailableSteps(GetAvailableStepsRequest request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            var result = new GetAvailableStepsResponse();
            foreach (SDK.Common.IStepProxy step in _pluginsService.Steps)
            {
                SDK.Common.Models.Step stepBinding = RuntimeMapper.FromRuntime(step);
                Step rpcStep = RpcMapper.ToRpc(stepBinding);
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

            IEnumerable<SDK.Common.IStepProxy> flowSteps = _flowService.GetSteps();
            foreach (SDK.Common.IStepProxy fs in flowSteps)
            {
                result.Steps.Add(RpcMapper.ToRpc(RuntimeMapper.FromRuntime(fs)));
            }

            return result;
        });
    }

    public override Task<GetFlowLinksResponse> GetFlowLinks(GetFlowLinksRequest request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            var result = new GetFlowLinksResponse();

            IEnumerable<SDK.Common.Ports.PortLink> flowLinks = _flowService.GetLinks();
            foreach (SDK.Common.Ports.PortLink fl in flowLinks)
            {
                result.Links.Add(RpcMapper.ToRpc(fl));
            }
            return result;
        });
    }

    public override async Task<GetFlowPortsResponse> GetFlowPorts(GetFlowPortsRequest request, ServerCallContext context)
    {
        var resultPorts = new List<Port>();
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

            SDK.Common.Ports.IPort port = _flowService.GetPort(portId);
            if (port == null)
            {
                _logger.LogWarning("Port not found: {PortId}", portId);
                continue;
            }

            if (iterationId != Guid.Empty)
            {
                resultPorts.Add(RpcMapper.ToRpc(_cacheService.GetOrCreatePortEntry(iterationId, port)));
            }
            else
            {
                resultPorts.Add(RpcMapper.ToRpc(RuntimeMapper.FromRuntime(port)));
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
            Step = RpcMapper.ToRpc(RuntimeMapper.FromRuntime(stepProxy))
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

            _= await _flowService.LinkPortsAsync(sourceId, targetId);
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
        SDK.Common.Models.Port port = RpcMapper.FromRpc(request.Port);
        if(!await _flowService.TryUpdatePortValueAsync(port.Id, port.Value!))
        {
            _logger.LogWarning("Could not update port: {PortId}", port.Id);
            throw new RpcException(new Status(StatusCode.Internal, "Could not update port"));
        }

        return new Empty();
    }
}
