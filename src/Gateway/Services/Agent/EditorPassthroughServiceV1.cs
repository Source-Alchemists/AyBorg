using Ayborg.Gateway.V1;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class EditorPassthroughServiceV1 : AgentEditor.AgentEditorBase
{
    private readonly ILogger<EditorPassthroughServiceV1> _logger;
    private readonly IGrpcChannelService _grpcChannelService;

    public EditorPassthroughServiceV1(ILogger<EditorPassthroughServiceV1> logger, IGrpcChannelService grpcChannelService)
    {
        _logger = logger;
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<GetAvailableStepsResponse> GetAvailableSteps(GetAvailableStepsRequest request, ServerCallContext context)
    {
        AgentEditor.AgentEditorClient client = _grpcChannelService.CreateClient<AgentEditor.AgentEditorClient>(request.AgentUniqueName);
        return await client.GetAvailableStepsAsync(request);
    }

    public override async Task<GetFlowStepsResponse> GetFlowSteps(GetFlowStepsRequest request, ServerCallContext context)
    {
        AgentEditor.AgentEditorClient client = _grpcChannelService.CreateClient<AgentEditor.AgentEditorClient>(request.AgentUniqueName);
        return await client.GetFlowStepsAsync(request);
    }

    public override async Task<GetFlowLinksResponse> GetFlowLinks(GetFlowLinksRequest request, ServerCallContext context)
    {
        AgentEditor.AgentEditorClient client = _grpcChannelService.CreateClient<AgentEditor.AgentEditorClient>(request.AgentUniqueName);
        return await client.GetFlowLinksAsync(request);
    }

    public override async Task<GetFlowPortsResponse> GetFlowPorts(GetFlowPortsRequest request, ServerCallContext context)
    {
        AgentEditor.AgentEditorClient client = _grpcChannelService.CreateClient<AgentEditor.AgentEditorClient>(request.AgentUniqueName);
        return await client.GetFlowPortsAsync(request);
    }
}
