using Atomy.SDK.Data.DAL;
using Atomy.SDK.Projects;

namespace Atomy.Agent.Services;
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
    Task<bool> TryActivateAsync(Guid projectId, bool isActive);

    /// <summary>
    /// Creates the asynchronous.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    Task<ProjectRecord> CreateAsync(string name);

    /// <summary>
    /// Deletes asynchronous.
    /// </summary>
    /// <param name="projectId">The project id.</param>
    /// <returns></returns>
    Task<bool> TryDeleteAsync(Guid projectId);

    /// <summary>
    /// Change project state asynchronous.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="state">The state.</param>
    /// <returns></returns>
    Task<bool> TryChangeProjectStateAsync(Guid projectId, ProjectState state);

    /// <summary>
    /// Gets all project metas asynchronous.
    /// </summary>
    Task<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync();

    /// <summary>
    /// Load active project asynchronous.
    /// </summary>
    Task<bool> TryLoadActiveProjectAsync();

    /// <summary>
    /// Save active project asynchronous.
    /// </summary>
    /// <returns></returns>
    Task<bool> TrySaveActiveProjectAsync();
}
