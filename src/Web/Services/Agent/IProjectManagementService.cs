using AyBorg.Web.Shared.Models.Agent;

namespace AyBorg.Web.Services.Agent;

public interface IProjectManagementService
{
    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <returns></returns>
    ValueTask<IEnumerable<ProjectMeta>> GetMetasAsync();

    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns></returns>
    ValueTask<IEnumerable<ProjectMeta>> GetMetasAsync(string serviceUniqueName);

    /// <summary>
    /// Receives active project meta asynchronous.
    /// </summary>
    /// <returns></returns>
    ValueTask<ProjectMeta> GetActiveMetaAsync();

    /// <summary>
    /// Receives active project meta asynchronous.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns></returns>
    ValueTask<ProjectMeta> GetActiveMetaAsync(string serviceUniqueName);

    /// <summary>
    /// Creates asynchronous.
    /// </summary>
    /// <param name="projectName">Name of the project.</param>
    /// <returns></returns>
    ValueTask<ProjectMeta> CreateAsync(string projectName);

    /// <summary>
    /// Deletes asynchronous.
    /// </summary>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    ValueTask<bool> TryDeleteAsync(ProjectMeta projectMeta);

    /// <summary>
    /// Set the state of the project to active.
    /// </summary>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    ValueTask<bool> TryActivateAsync(ProjectMeta projectMeta);

    /// <summary>
    /// Sets the project to ready state.
    /// </summary>
    /// <param name="projectMeta">The project meta info.</param>
    /// <param name="projectSaveInfo">State of the project.</param>
    /// <returns></returns>
    ValueTask<bool> TryApproveAsync(ProjectMeta projectMeta, ProjectSaveInfo projectSaveInfo);

    /// <summary>
    /// Save the project.
    /// </summary>
    /// <param name="projectMeta">The project meta.</param>
    /// <param name="projectSaveInfo">State of the project.</param>
    /// <returns></returns>
    ValueTask<bool> TrySaveAsync(ProjectMeta projectMeta, ProjectSaveInfo projectSaveInfo);
}
