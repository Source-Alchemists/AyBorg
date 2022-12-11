using AyBorg.Database.Data;
using AyBorg.SDK.Data.DAL;
using AyBorg.SDK.Projects;
using Microsoft.EntityFrameworkCore;

namespace AyBorg.Agent.Services;

public sealed class ProjectSettingsService : IProjectSettingsService
{
    private readonly ILogger<ProjectSettingsService> _logger;
    private readonly IDbContextFactory<ProjectContext> _projectContextFactory;
    private readonly IProjectManagementService _projectManagementService;
    private readonly IEngineHost _engineHost;

    public ProjectSettingsService(ILogger<ProjectSettingsService> logger, IDbContextFactory<ProjectContext> projectContextFactory, IProjectManagementService projectManagementService, IEngineHost engineHost)
    {
        _logger = logger;
        _projectContextFactory = projectContextFactory;
        _projectManagementService = projectManagementService;
        _engineHost = engineHost;
    }

    /// <summary>
    /// Gets the project settings asynchronous.
    /// </summary>
    /// <param name="projectMetaDbId">The project meta database identifier.</param>
    /// <returns></returns>
    public async ValueTask<ProjectSettingsRecord> GetSettingsRecordAsync(Guid projectMetaDbId)
    {
        using ProjectContext context = await _projectContextFactory.CreateDbContextAsync();
        ProjectRecord? projectRecord = await context.AyBorgProjects!.Include(x => x.Settings).FirstOrDefaultAsync(x => x.Meta.DbId.Equals(projectMetaDbId));
        if (projectRecord == null)
        {
            _logger.LogWarning("No project found with id [{projectDatabaseId}].", projectMetaDbId);
            return null!;
        }

        return projectRecord.Settings;
    }

    /// <summary>
    /// Tries to update the project settings asynchronous.
    /// </summary>
    /// <param name="projectMetaDbId">The project meta database identifier.</param>
    /// <param name="projectSettings">The project settings.</param>
    /// <returns></returns>
    public async ValueTask<bool> TryUpdateActiveProjectSettingsAsync(Guid projectMetaDbId, ProjectSettings projectSettings)
    {
        IEnumerable<ProjectMetaRecord> projectMetas = await _projectManagementService.GetAllMetasAsync();
        ProjectMetaRecord? projectMeta = projectMetas.FirstOrDefault(p => p.DbId == projectMetaDbId);
        if (projectMeta == null)
        {
            _logger.LogWarning("No settings found for project {projectMetaDbId}.", projectMetaDbId);
            return false;
        }

        if (_projectManagementService.ActiveProjectId == projectMeta.Id)
        {
            _engineHost.ActiveProject!.Settings.IsForceResultCommunicationEnabled = projectSettings.IsForceResultCommunicationEnabled;
            _engineHost.ActiveProject!.Settings.IsForceWebUiCommunicationEnabled = projectSettings.IsForceWebUiCommunicationEnabled;
        }

        return true;
    }
}
