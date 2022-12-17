using Ayborg.Gateway.Agent.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Agent.Services.gRPC;

public sealed class ProjectSettingsServiceV1 : AgentProjectSettings.AgentProjectSettingsBase
{
    private readonly ILogger<ProjectManagementServiceV1> _logger;
    private readonly IProjectSettingsService _projectSettingsService;

    public ProjectSettingsServiceV1(ILogger<ProjectManagementServiceV1> logger, IProjectSettingsService projectSettingsService)
    {
        _logger = logger;
        _projectSettingsService = projectSettingsService;
    }

    public override async Task<GetProjectSettingsResponse> GetProjectSettings(GetProjectSettingsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectDbId, out Guid dbId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProjectDbId is not a valid GUID"));

        }
        SDK.Data.DAL.ProjectSettingsRecord projectSettingsRecord = await _projectSettingsService.GetSettingsRecordAsync(dbId);

        return new GetProjectSettingsResponse
        {
            ProjectSettings = new ProjectSettings
            {
                IsForceResultCommunicationEnabled = projectSettingsRecord.IsForceResultCommunicationEnabled,
                IsForceWebUiCommunicationEnabled = projectSettingsRecord.IsForceWebUiCommunicationEnabled
            }
        };
    }

    public override async Task<Empty> UpdateProjectSettings(UpdateProjectSettingsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectDbId, out Guid dbId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProjectDbId is not a valid GUID"));
        }

        ProjectSettings settings = request.ProjectSettings;

        if (!await _projectSettingsService.TryUpdateActiveProjectSettingsAsync(dbId, new SDK.Projects.ProjectSettings
        {
            IsForceResultCommunicationEnabled = settings.IsForceResultCommunicationEnabled,
            IsForceWebUiCommunicationEnabled = settings.IsForceWebUiCommunicationEnabled
        }))
        {
            throw new RpcException(new Status(StatusCode.Internal, "Could not update project settings"));
        }

        return new Empty();
    }
}
