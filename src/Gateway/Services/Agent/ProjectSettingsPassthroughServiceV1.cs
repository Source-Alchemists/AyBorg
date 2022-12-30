using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Authorization;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Gateway.Services.Agent;

public sealed class ProjectSettingsPassthroughServiceV1 : ProjectSettings.ProjectSettingsBase
{
    private readonly ILogger<ProjectManagementPassthroughServiceV1> _logger;
    private readonly IGrpcChannelService _grpcChannelService;

    public ProjectSettingsPassthroughServiceV1(ILogger<ProjectManagementPassthroughServiceV1> logger, IGrpcChannelService grpcChannelService)
    {
        _logger = logger;
        _grpcChannelService = grpcChannelService;
    }

    public override async Task<GetProjectSettingsResponse> GetProjectSettings(GetProjectSettingsRequest request, ServerCallContext context)
    {
        ProjectSettings.ProjectSettingsClient client = _grpcChannelService.CreateClient<ProjectSettings.ProjectSettingsClient>(request.AgentUniqueName);
        return await client.GetProjectSettingsAsync(request);
    }

    public override async Task<Empty> UpdateProjectSettings(UpdateProjectSettingsRequest request, ServerCallContext context)
    {
        Metadata headers = AuthorizeUtil.Protect(context.GetHttpContext(), new List<string> { Roles.Administrator });
        ProjectSettings.ProjectSettingsClient client = _grpcChannelService.CreateClient<ProjectSettings.ProjectSettingsClient>(request.AgentUniqueName);
        return await client.UpdateProjectSettingsAsync(request, headers);
    }
}
