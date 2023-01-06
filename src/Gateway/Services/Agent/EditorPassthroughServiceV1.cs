using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Authorization;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class EditorPassthroughServiceV1 : Editor.EditorBase
{
    private readonly IGrpcChannelService _grpcChannelService;

    public EditorPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
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
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.AddFlowStepAsync(request, headers);
    }

    public override async Task<Empty> DeleteFlowStep(DeleteFlowStepRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.DeleteFlowStepAsync(request, headers);
    }

    public override async Task<Empty> MoveFlowStep(MoveFlowStepRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.MoveFlowStepAsync(request, headers);
    }

    public override async Task<LinkFlowPortsResponse> LinkFlowPorts(LinkFlowPortsRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.LinkFlowPortsAsync(request, headers);
    }

    public override async Task<Empty> UpdateFlowPort(UpdateFlowPortRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        Editor.EditorClient client = _grpcChannelService.CreateClient<Editor.EditorClient>(request.AgentUniqueName);
        return await client.UpdateFlowPortAsync(request, headers);
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
