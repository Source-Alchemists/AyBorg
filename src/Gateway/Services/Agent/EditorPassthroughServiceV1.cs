using Ayborg.Gateway.Agent.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class EditorPassthroughServiceV1 : Editor.EditorBase
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
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.GetAvailableStepsAsync(request);
    }

    public override async Task<GetFlowStepsResponse> GetFlowSteps(GetFlowStepsRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.GetFlowStepsAsync(request);
    }

    public override async Task<GetFlowLinksResponse> GetFlowLinks(GetFlowLinksRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.GetFlowLinksAsync(request);
    }

    public override async Task<GetFlowPortsResponse> GetFlowPorts(GetFlowPortsRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.GetFlowPortsAsync(request);
    }

    public override async Task<AddFlowStepResponse> AddFlowStep(AddFlowStepRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.AddFlowStepAsync(request);
    }

    public override async Task<Empty> DeleteFlowStep(DeleteFlowStepRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.DeleteFlowStepAsync(request);
    }

    public override async Task<Empty> MoveFlowStep(MoveFlowStepRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.MoveFlowStepAsync(request);
    }

    public override async Task<Empty> LinkFlowPorts(LinkFlowPortsRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.LinkFlowPortsAsync(request);
    }

    public override async Task<Empty> UpdateFlowPort(UpdateFlowPortRequest request, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.UpdateFlowPortAsync(request);
    }

    public override async Task GetImageStream(GetImageStreamRequest request, IServerStreamWriter<ImageChunkDto> responseStream, ServerCallContext context)
    {
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);

        AsyncServerStreamingCall<ImageChunkDto> stream = client.GetImageStream(request);
        await foreach (ImageChunkDto? chunk in stream.ResponseStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(chunk);
        }
    }
}
