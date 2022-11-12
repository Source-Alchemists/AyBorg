using AyBorg.SDK.Data.DTOs;

namespace AyBorg.Web.Services.Agent;

public interface IProjectManagementService
{
    /// <summary>
    /// Receives asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns></returns>
    Task<IEnumerable<ProjectMetaDto>> GetMetasAsync(string baseUrl);

    /// <summary>
    /// Receives active project meta asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns></returns>
    Task<ProjectMetaDto> GetActiveMetaAsync(string baseUrl);

    /// <summary>
    /// Creates asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectName">Name of the project.</param>
    /// <returns></returns>
    Task<ProjectMetaDto> CreateAsync(string baseUrl, string projectName);

    /// <summary>
    /// Deletes asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    Task<bool> TryDeleteAsync(string baseUrl, ProjectMetaDto projectMeta);

    /// <summary>
    /// Set the state of the project to active.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    Task<bool> TryActivateAsync(string baseUrl, ProjectMetaDto projectMeta);

    /// <summary>
    /// Sets the project to ready state.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    Task<bool> TrySaveAsReadyAsync(string baseUrl, ProjectMetaDto projectMeta);

    /// <summary>
    /// Save the project.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="projectMeta">The project meta.</param>
    /// <returns></returns>
    Task<bool> TrySaveAsync(string baseUrl, ProjectMetaDto projectMeta);
}