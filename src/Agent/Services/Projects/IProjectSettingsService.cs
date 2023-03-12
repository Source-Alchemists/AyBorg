using AyBorg.Data.Agent;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Services;

public interface IProjectSettingsService
{
    /// <summary>
    /// Gets the project settings asynchronous.
    /// </summary>
    /// <param name="projectMetaDbId">The project meta database identifier.</param>
    /// <returns></returns>
    ValueTask<ProjectSettingsRecord> GetSettingsRecordAsync(Guid projectMetaDbId);

    /// <summary>
    /// Tries to update the project settings asynchronous.
    /// </summary>
    /// <param name="projectMetaDbId">The project meta database identifier.</param>
    /// <param name="projectSettings">The project settings.</param>
    /// <returns></returns>
    ValueTask<bool> TryUpdateActiveProjectSettingsAsync(Guid projectMetaDbId, ProjectSettings projectSettings);
}
