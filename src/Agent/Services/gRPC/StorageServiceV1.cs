using Ayborg.Gateway.Agent.V1;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class StorageServiceV1 : Storage.StorageBase
{
    private readonly ILogger<StorageServiceV1> _logger;
    private readonly IStorageService _storageService;

    public StorageServiceV1(ILogger<StorageServiceV1> logger, IStorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
    }

    public override Task<GetDirectoriesResponse> GetDirectories(GetDirectoriesRequest request, ServerCallContext context)
    {
        return Task.Factory.StartNew(() =>
        {
            IEnumerable<string> directories = _storageService.GetDirectories(request.Path);
            var result = new GetDirectoriesResponse();
            result.Directories.Add(directories);
            return result;
        });
    }
}