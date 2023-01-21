using Ayborg.Gateway.Agent.V1;
using AyBorg.Data.Agent;
using AyBorg.SDK.Authorization;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class ProjectSettingsServiceV1 : ProjectSettings.ProjectSettingsBase
{
    private readonly IProjectSettingsService _projectSettingsService;

    public ProjectSettingsServiceV1(IProjectSettingsService projectSettingsService)
    {
        _projectSettingsService = projectSettingsService;
    }

    public override async Task<GetProjectSettingsResponse> GetProjectSettings(GetProjectSettingsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectDbId, out Guid dbId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProjectDbId is not a valid GUID"));

        }
        ProjectSettingsRecord projectSettingsRecord = await _projectSettingsService.GetSettingsRecordAsync(dbId);

        return new GetProjectSettingsResponse
        {
            ProjectSettings = new ProjectSettingsDto
            {
                IsForceResultCommunicationEnabled = projectSettingsRecord.IsForceResultCommunicationEnabled
            }
        };
    }

    public override async Task<Empty> UpdateProjectSettings(UpdateProjectSettingsRequest request, ServerCallContext context)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(context.GetHttpContext(), new List<string> { Roles.Administrator });
        if (!Guid.TryParse(request.ProjectDbId, out Guid dbId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProjectDbId is not a valid GUID"));
        }

        ProjectSettingsDto settings = request.ProjectSettings;

        if (!await _projectSettingsService.TryUpdateActiveProjectSettingsAsync(dbId, new SDK.Projects.ProjectSettings
        {
            IsForceResultCommunicationEnabled = settings.IsForceResultCommunicationEnabled
        }))
        {
            throw new RpcException(new Status(StatusCode.Internal, "Could not update project settings"));
        }

        return new Empty();
    }
}
