using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Authorization;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class StorageServiceV1 : Storage.StorageBase
{
    private readonly IStorageService _storageService;

    public StorageServiceV1(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public override Task<GetDirectoriesResponse> GetDirectories(GetDirectoriesRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer, Roles.Reviewer, Roles.Auditor });
        return Task.Factory.StartNew(() =>
        {
            IEnumerable<string> directories = _storageService.GetDirectories(request.Path);
            var result = new GetDirectoriesResponse();
            result.Directories.Add(directories);
            return result;
        });
    }
}
