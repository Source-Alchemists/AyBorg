using AyBorg.SDK.Data.DAL;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Services;
public interface IProjectManagementService
{
    /// <summary>
    /// Gets the active project identifier.
    /// </summary>
    Guid ActiveProjectId { get; }

    /// <summary>
    /// Activates asynchronous.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="isActive">if set to <c>true</c> [is active].</param>
    /// <returns></returns>
    ValueTask<bool> TryActivateAsync(Guid projectId, bool isActive);

    /// <summary>
    /// Creates the asynchronous.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    ValueTask<ProjectRecord> CreateAsync(string name);

    /// <summary>
    /// Deletes asynchronous.
    /// </summary>
    /// <param name="projectId">The project id.</param>
    /// <returns></returns>
    ValueTask<bool> TryDeleteAsync(Guid projectId);

    /// <summary>
    /// Change project state asynchronous.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="state">The state.</param>
    /// <returns></returns>
    ValueTask<bool> TryChangeProjectStateAsync(Guid projectId, ProjectState state);

    /// <summary>
    /// Gets all project metas asynchronous.
    /// </summary>
    ValueTask<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync();

    /// <summary>
    /// Load active project asynchronous.
    /// </summary>
    ValueTask<bool> TryLoadActiveProjectAsync();

    /// <summary>
    /// Save active project asynchronous.
    /// </summary>
    /// <returns></returns>
    ValueTask<bool> TrySaveActiveProjectAsync();
}
