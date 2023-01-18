using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Authorization;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class StoragePassthroughServiceV1 : Storage.StorageBase
{
    private readonly IGrpcChannelService _grpcChannelService;

    public StoragePassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<GetDirectoriesResponse> GetDirectories(GetDirectoriesRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer });
        Storage.StorageClient client = _grpcChannelService.CreateClient<Storage.StorageClient>(request.AgentUniqueName);
        return await client.GetDirectoriesAsync(request, headers);
    }
}
