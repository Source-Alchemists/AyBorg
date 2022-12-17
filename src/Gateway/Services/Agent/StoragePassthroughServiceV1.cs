using Ayborg.Gateway.Agent.V1;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class StoragePassthroughServiceV1 : Storage.StorageBase
{
    private readonly ILogger<StoragePassthroughServiceV1> _logger;
    private readonly IGrpcChannelService _grpcChannelService;

    public StoragePassthroughServiceV1(ILogger<StoragePassthroughServiceV1> logger, IGrpcChannelService grpcChannelService)
    {
        _logger = logger;
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<GetDirectoriesResponse> GetDirectories(GetDirectoriesRequest request, ServerCallContext context)
    {
        Storage.StorageClient client = _grpcChannelService.CreateClient<Storage.StorageClient>(request.AgentUniqueName);
        return await client.GetDirectoriesAsync(request);
    }
}
