using Ayborg.Gateway.Agent.V1;
using AyBorg.SDK.Common;
using Grpc.Core;

namespace AyBorg.Web.Services.Agent;

public sealed class ProjectSettingsService : IProjectSettingsService
{
    private readonly ILogger<ProjectSettingsService> _logger;
    private readonly ProjectSettings.ProjectSettingsClient _projectSettingsClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectSettingsService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="projectSettingsClient">The project settings client.</param>
    public ProjectSettingsService(ILogger<ProjectSettingsService> logger,
                                    ProjectSettings.ProjectSettingsClient projectSettingsClient)
    {
        _logger = logger;
        _projectSettingsClient = projectSettingsClient;
    }

    /// <summary>
    /// Gets the project settings asynchronous.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="projectMeta">The project meta info.</param>
    /// <returns></returns>
    public async ValueTask<Shared.Models.Agent.ProjectSettings> GetProjectSettingsAsync(string agentUniqueName, Shared.Models.Agent.ProjectMeta projectMeta)
    {
        try
        {
            GetProjectSettingsResponse response = await _projectSettingsClient.GetProjectSettingsAsync(new GetProjectSettingsRequest
            {
                AgentUniqueName = agentUniqueName,
                ProjectDbId = projectMeta.DbId
            });

            return new Shared.Models.Agent.ProjectSettings(response.ProjectSettings);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to get project settings");
            return null!;
        }
    }

    /// <summary>
    /// Updates the project communication settings asynchronous.
    /// </summary>
    /// <param name="agentUniqueName">The unique name.</param>
    /// <param name="projectMeta">The project meta info.</param>
    /// <param name="projectSettings">The project settings.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryUpdateProjectCommunicationSettingsAsync(string agentUniqueName, Shared.Models.Agent.ProjectMeta projectMeta, Shared.Models.Agent.ProjectSettings projectSettings)
    {
        try
        {
            _logger.LogInformation(new EventId((int)EventLogType.UserInteraction), "Updating project communication settings for project [{projectName}]: {projectSettings}", projectMeta.Name, projectSettings);
            _ = await _projectSettingsClient.UpdateProjectSettingsAsync(new UpdateProjectSettingsRequest
            {
                AgentUniqueName = agentUniqueName,
                ProjectDbId = projectMeta.DbId,
                ProjectSettings = new ProjectSettingsDto
                {
                    IsForceResultCommunicationEnabled = projectSettings.IsForceResultCommunicationEnabled
                }
            });

            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.UserInteraction), ex, "Failed to update project communication settings");
            return false;
        }
    }
}
