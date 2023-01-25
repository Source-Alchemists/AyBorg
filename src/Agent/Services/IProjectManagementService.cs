using AyBorg.Data.Agent;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Services;

public interface IProjectManagementService
{
    /// <summary>
    /// Gets the active project identifier.
    /// </summary>
    Guid ActiveProjectId { get; }

    /// <summary>
    /// Changes the activation state.
    /// </summary>
    /// <param name="projectMetaDbId">The project database identifier.</param>
    /// <param name="isActive">if set to <c>true</c> [is active].</param>
    /// <returns></returns>
    ValueTask<ProjectManagementResult> TryChangeActivationStateAsync(Guid projectMetaDbId, bool isActive);

    /// <summary>
    /// Creates the project.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    ValueTask<ProjectRecord> CreateAsync(string name);

    /// <summary>
    /// Deletes the project.
    /// </summary>
    /// <param name="projectMetaId">The project meta id.</param>
    /// <returns></returns>
    ValueTask<ProjectManagementResult> TryDeleteAsync(Guid projectMetaId);

    /// <summary>
    /// Gets all project metas.
    /// </summary>
    ValueTask<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync();

    /// <summary>
    /// Load active project.
    /// </summary>
    ValueTask<ProjectManagementResult> TryLoadActiveAsync();

    /// <summary>
    /// Save active project.
    /// </summary>
    /// <returns></returns>
    ValueTask<ProjectManagementResult> TrySaveActiveAsync();

    /// <summary>
    /// Save the project as new version.
    /// </summary>
    ValueTask<ProjectManagementResult> TrySaveNewVersionAsync(Guid projectMetaDbId, ProjectState projectState, string newVersionName, string comment, string? approver = null);
}
