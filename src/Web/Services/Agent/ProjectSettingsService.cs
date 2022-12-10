using Ayborg.Gateway.V1;
using Grpc.Core;

namespace AyBorg.Web.Services.Agent;

public sealed class ProjectSettingsService : IProjectSettingsService
{
    private readonly ILogger<ProjectSettingsService> _logger;
    private readonly IAuthorizationHeaderUtilService _authorizationHeaderUtilService;
    private readonly AgentProjectSettings.AgentProjectSettingsClient _agentProjectSettingsClient;

    public ProjectSettingsService(ILogger<ProjectSettingsService> logger,
                                    IAuthorizationHeaderUtilService authorizationHeaderUtilService,
                                    AgentProjectSettings.AgentProjectSettingsClient agentProjectSettingsClient)
    {
        _logger = logger;
        _authorizationHeaderUtilService = authorizationHeaderUtilService;
        _agentProjectSettingsClient = agentProjectSettingsClient;
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
            GetProjectSettingsResponse response = await _agentProjectSettingsClient.GetProjectSettingsAsync(new GetProjectSettingsRequest
            {
                AgentUniqueName = agentUniqueName,
                ProjectDbId = projectMeta.DbId
            });

            return new Shared.Models.Agent.ProjectSettings(response.ProjectSettings);
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to get project settings");
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
            _ = await _agentProjectSettingsClient.UpdateProjectSettingsAsync(new UpdateProjectSettingsRequest
            {
                AgentUniqueName = agentUniqueName,
                ProjectDbId = projectMeta.DbId,
                ProjectSettings = new ProjectSettings
                {
                    IsForceResultCommunicationEnabled = projectSettings.IsForceResultCommunicationEnabled,
                    IsForceWebUiCommunicationEnabled = projectSettings.IsForceWebUiCommunicationEnabled
                }
            });

            return true;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to update project communication settings");
            return false;
        }
    }
}
