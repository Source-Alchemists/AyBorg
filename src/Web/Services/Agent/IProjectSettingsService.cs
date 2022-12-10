namespace AyBorg.Web.Services.Agent;

public interface IProjectSettingsService
{
    /// <summary>
    /// Gets the project settings asynchronous.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="projectMeta">The project meta info.</param>
    /// <returns></returns>
    ValueTask<Shared.Models.Agent.ProjectSettings> GetProjectSettingsAsync(string agentUniqueName, Shared.Models.Agent.ProjectMeta projectMeta);

    /// <summary>
    /// Updates the project communication settings asynchronous.
    /// </summary>
    /// <param name="agentUniqueName">The unique name.</param>
    /// <param name="projectMeta">The project meta info.</param>
    /// <param name="projectSettings">The project settings.</param>
    ValueTask<bool> TryUpdateProjectCommunicationSettingsAsync(string agentUniqueName, Shared.Models.Agent.ProjectMeta projectMeta, Shared.Models.Agent.ProjectSettings projectSettings);
}
