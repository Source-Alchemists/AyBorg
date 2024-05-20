using Ayborg.Gateway.Cognitive.Agent.V1;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Cognitive.Agent;

public sealed class StatusManagerPassthroughServiceV1 : StatusManager.StatusManagerBase
{
    private readonly IGrpcChannelService _channelService;

    public StatusManagerPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }

    public override async Task<ServiceStatus> Get(GetStatusRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), null!);
        StatusManager.StatusManagerClient client = _channelService.CreateClient<StatusManager.StatusManagerClient>(request.ServiceUniqueName);
        return await client.GetAsync(request, headers);
    }
}
