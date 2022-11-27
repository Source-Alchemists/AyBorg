using AyBorg.SDK.Data.DTOs;

namespace AyBorg.Web.Services.Agent;

public interface IProjectManagementService
{
    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns></returns>
    ValueTask<IEnumerable<ProjectMetaDto>> GetMetasAsync(string baseUrl);

    /// <summary>
    /// Receives active project meta asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns></returns>
    ValueTask<ProjectMetaDto> GetActiveMetaAsync(string baseUrl);

    /// <summary>
    /// Creates asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectName">Name of the project.</param>
    /// <returns></returns>
    ValueTask<ProjectMetaDto> CreateAsync(string baseUrl, string projectName);

    /// <summary>
    /// Deletes asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    ValueTask<bool> TryDeleteAsync(string baseUrl, ProjectMetaDto projectMeta);

    /// <summary>
    /// Set the state of the project to active.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    ValueTask<bool> TryActivateAsync(string baseUrl, ProjectMetaDto projectMeta);

    /// <summary>
    /// Sets the project to ready state.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="dbId">The database identifier.</param>
    /// <param name="projectStateChange">State of the project.</param>
    /// <returns></returns>
    ValueTask<bool> TrySaveNewVersionAsync(string baseUrl, Guid dbId, ProjectStateChangeDto projectStateChange);

    /// <summary>
    /// Sets the project to ready state.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="dbId">The database identifier.</param>
    /// <param name="projectStateChange">State of the project.</param>
    /// <returns></returns>
    ValueTask<bool> TryApproveAsnyc(string baseUrl, Guid dbId, ProjectStateChangeDto projectStateChange);

    /// <summary>
    /// Save the project.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    ValueTask<bool> TrySaveAsync(string baseUrl, ProjectMetaDto projectMeta);

    /// <summary>
    /// Gets the project settings asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta info.</param>
    /// <returns></returns>
    ValueTask<ProjectSettingsDto> GetProjectSettingsAsync(string baseUrl, ProjectMetaDto projectMeta);

    /// <summary>
    /// Updates the project communication settings asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta info.</param>
    /// <param name="projectSettings">The project settings.</param>
    /// <returns></returns>
    ValueTask<bool> TryUpdateProjectCommunicationSettingsAsync(string baseUrl, ProjectMetaDto projectMeta, ProjectSettingsDto projectSettings);
}
