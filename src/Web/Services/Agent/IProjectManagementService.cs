namespace AyBorg.Web.Services.Agent;

public interface IProjectManagementService
{
    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <returns></returns>
    ValueTask<IEnumerable<Shared.Models.Agent.ProjectMeta>> GetMetasAsync();

    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns></returns>
    ValueTask<IEnumerable<Shared.Models.Agent.ProjectMeta>> GetMetasAsync(string serviceUniqueName);

    /// <summary>
    /// Receives active project meta asynchronous.
    /// </summary>
    /// <returns></returns>
    ValueTask<Shared.Models.Agent.ProjectMeta> GetActiveMetaAsync();

    /// <summary>
    /// Receives active project meta asynchronous.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns></returns>
    ValueTask<Shared.Models.Agent.ProjectMeta> GetActiveMetaAsync(string serviceUniqueName);

    /// <summary>
    /// Creates asynchronous.
    /// </summary>
    /// <param name="projectName">Name of the project.</param>
    /// <returns></returns>
    ValueTask<Shared.Models.Agent.ProjectMeta> CreateAsync(string projectName);

    /// <summary>
    /// Deletes asynchronous.
    /// </summary>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    ValueTask<bool> TryDeleteAsync(Shared.Models.Agent.ProjectMeta projectMeta);

    /// <summary>
    /// Set the state of the project to active.
    /// </summary>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    ValueTask<bool> TryActivateAsync(Shared.Models.Agent.ProjectMeta projectMeta);

    /// <summary>
    /// Sets the project to ready state.
    /// </summary>
    /// <param name="dbId">The database identifier.</param>
    /// <param name="projectSaveInfo">State of the project.</param>
    /// <returns></returns>
    ValueTask<bool> TryApproveAsync(string dbId, Shared.Models.Agent.ProjectSaveInfo projectSaveInfo);

    /// <summary>
    /// Save the project.
    /// </summary>
    /// <param name="projectMeta">The project meta.</param>
    /// <param name="projectSaveInfo">State of the project.</param>
    /// <returns></returns>
    ValueTask<bool> TrySaveAsync(Shared.Models.Agent.ProjectMeta projectMeta, Shared.Models.Agent.ProjectSaveInfo projectSaveInfo);
}
