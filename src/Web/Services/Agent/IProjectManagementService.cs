namespace AyBorg.Web.Services.Agent;

public interface IProjectManagementService
{
    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <returns></returns>
    ValueTask<IEnumerable<Shared.Models.Agent.ProjectMeta>> GetMetasAsync(string agentUniqueName);

    /// <summary>
    /// Receives active project meta asynchronous.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <returns></returns>
    ValueTask<Shared.Models.Agent.ProjectMeta> GetActiveMetaAsync(string agentUniqueName);

    /// <summary>
    /// Creates asynchronous.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="projectName">Name of the project.</param>
    /// <returns></returns>
    ValueTask<Shared.Models.Agent.ProjectMeta> CreateAsync(string agentUniqueName, string projectName);

    /// <summary>
    /// Deletes asynchronous.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    ValueTask<bool> TryDeleteAsync(string agentUniqueName, Shared.Models.Agent.ProjectMeta projectMeta);

    /// <summary>
    /// Set the state of the project to active.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    ValueTask<bool> TryActivateAsync(string agentUniqueName, Shared.Models.Agent.ProjectMeta projectMeta);

    /// <summary>
    /// Sets the project to ready state.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="dbId">The database identifier.</param>
    /// <param name="projectSaveInfo">State of the project.</param>
    /// <returns></returns>
    ValueTask<bool> TryApproveAsync(string agentUniqueName, string dbId, Shared.Models.Agent.ProjectSaveInfo projectSaveInfo);

    /// <summary>
    /// Save the project.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique.</param>
    /// <param name="projectMeta">The project meta.</param>
    /// <param name="projectSaveInfo">State of the project.</param>
    /// <returns></returns>
    ValueTask<bool> TrySaveAsync(string agentUniqueName, Shared.Models.Agent.ProjectMeta projectMeta, Shared.Models.Agent.ProjectSaveInfo projectSaveInfo);
}
