using Ayborg.Gateway.V1;
using Google.Protobuf.WellKnownTypes;
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

    public override async Task<AddFlowStepResponse> AddFlowStep(AddFlowStepRequest request, ServerCallContext context)
    {
        AgentEditor.AgentEditorClient client = _grpcChannelService.CreateClient<AgentEditor.AgentEditorClient>(request.AgentUniqueName);
        return await client.AddFlowStepAsync(request);
    }

    public override async Task<Empty> DeleteFlowStep(DeleteFlowStepRequest request, ServerCallContext context)
    {
        AgentEditor.AgentEditorClient client = _grpcChannelService.CreateClient<AgentEditor.AgentEditorClient>(request.AgentUniqueName);
        return await client.DeleteFlowStepAsync(request);
    }

    public override async Task<Empty> MoveFlowStep(MoveFlowStepRequest request, ServerCallContext context)
    {
        AgentEditor.AgentEditorClient client = _grpcChannelService.CreateClient<AgentEditor.AgentEditorClient>(request.AgentUniqueName);
        return await client.MoveFlowStepAsync(request);
    }

    public override async Task<Empty> LinkFlowPorts(LinkFlowPortsRequest request, ServerCallContext context)
    {
        AgentEditor.AgentEditorClient client = _grpcChannelService.CreateClient<AgentEditor.AgentEditorClient>(request.AgentUniqueName);
        return await client.LinkFlowPortsAsync(request);
    }

    public override async Task<Empty> UpdateFlowPort(UpdateFlowPortRequest request, ServerCallContext context)
    {
        AgentEditor.AgentEditorClient client = _grpcChannelService.CreateClient<AgentEditor.AgentEditorClient>(request.AgentUniqueName);
        return await client.UpdateFlowPortAsync(request);
    }
}
