using Ayborg.Gateway.Net.V1;
using AyBorg.SDK.Authorization;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Net;

public sealed class ProjectManagerPassthroughServiceV1 : ProjectManager.ProjectManagerBase
{
    private readonly IGrpcChannelService _channelService;

    public ProjectManagerPassthroughServiceV1(IGrpcChannelService grpcChannelService)
    {
        _channelService = grpcChannelService;
    }

    public override async Task<ProjectMeta> Create(CreateProjectRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator, Roles.Engineer });
        ProjectManager.ProjectManagerClient client = _channelService.CreateClient<ProjectManager.ProjectManagerClient>(request.ServiceUniqueName);
        return await client.CreateAsync(request, headers);
    }
}