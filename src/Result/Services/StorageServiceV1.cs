using Ayborg.Gateway.Result.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Result.Services;

public sealed class StorageServiceV1 : Storage.StorageBase
{
    private readonly ILogger<StorageServiceV1> _logger;

    public StorageServiceV1(ILogger<StorageServiceV1> logger)
    {
        _logger = logger;
    }

    public override Task<Empty> Add(AddRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received result");
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> AddImage(IAsyncStreamReader<ImageChunkDto> requestStream, ServerCallContext context)
    {
        _logger.LogInformation("Received image");
        return Task.FromResult(new Empty());
    }
}
