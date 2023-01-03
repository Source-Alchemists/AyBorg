using AyBorg.Database.Data;
using AyBorg.SDK.Data.DAL;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Services;

public sealed class ProjectSettingsService : IProjectSettingsService
{
    private readonly ILogger<ProjectSettingsService> _logger;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectManagementService _projectManagementService;
    private readonly IEngineHost _engineHost;

    public ProjectSettingsService(ILogger<ProjectSettingsService> logger, IProjectRepository projectRepository, IProjectManagementService projectManagementService, IEngineHost engineHost)
    {
        _logger = logger;
        _projectRepository = projectRepository;
        _projectManagementService = projectManagementService;
        _engineHost = engineHost;
    }

    /// <summary>
    /// Gets the project settings asynchronous.
    /// </summary>
    /// <param name="projectMetaDbId">The project meta database identifier.</param>
    /// <returns></returns>
    public ValueTask<ProjectSettingsRecord> GetSettingsRecordAsync(Guid projectMetaDbId)
    {
        return _projectRepository.GetSettingAsync(projectMetaDbId);
    }

    /// <summary>
    /// Tries to update the project settings asynchronous.
    /// </summary>
    /// <param name="projectMetaDbId">The project meta database identifier.</param>
    /// <param name="projectSettings">The project settings.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryUpdateActiveProjectSettingsAsync(Guid projectMetaDbId, ProjectSettings projectSettings)
    {
        IEnumerable<ProjectMetaRecord> projectMetas = await  _projectRepository.GetAllMetasAsync();
        ProjectMetaRecord? projectMeta = projectMetas.FirstOrDefault(p => p.DbId == projectMetaDbId);
        if (projectMeta == null)
        {
            _logger.LogWarning("No settings found for project {projectMetaDbId}.", projectMetaDbId);
            return false;
        }

        if (_projectManagementService.ActiveProjectId == projectMeta.Id)
        {
            _engineHost.ActiveProject!.Settings.IsForceResultCommunicationEnabled = projectSettings.IsForceResultCommunicationEnabled;
        }

        return true;
    }
}
