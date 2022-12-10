using AyBorg.Database.Data;
using AyBorg.SDK.Data.DAL;
using AyBorg.SDK.Data.Mapper;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace AyBorg.Agent.Services;

internal sealed class ProjectManagementService : IProjectManagementService
{
    private readonly ILogger<ProjectManagementService> _logger;
    private readonly IDbContextFactory<ProjectContext> _projectContextFactory;
    private readonly IEngineHost _engineHost;
    private readonly IRuntimeToStorageMapper _runtimeToStorageMapper;
    private readonly IRuntimeConverterService _runtimeConverterService;
    private readonly string _serviceUniqueName;

    /// <summary>
    /// Gets the active project identifier.
    /// </summary>
    public Guid ActiveProjectId
    {
        get
        {
            if (_engineHost.ActiveProject == null)
            {
                return Guid.Empty;
            }
            return _engineHost.ActiveProject.Meta.Id;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectManagementService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceConfiguration">The service configuration.</param>
    /// <param name="projectContextFactory">The project context.</param>
    /// <param name="runtimeHost">The runtime host.</param>
    /// <param name="runtimeToStorageMapper">The runtime to storage mapper.</param>
    /// <param name="runtimeConverterService">The runtime converter service.</param>
    public ProjectManagementService(ILogger<ProjectManagementService> logger,
                                    IGatewayConfiguration serviceConfiguration,
                                    IDbContextFactory<ProjectContext> projectContextFactory,
                                    IEngineHost runtimeHost,
                                    IRuntimeToStorageMapper runtimeToStorageMapper,
                                    IRuntimeConverterService runtimeConverterService)
    {
        _logger = logger;
        _projectContextFactory = projectContextFactory;
        _engineHost = runtimeHost;
        _runtimeToStorageMapper = runtimeToStorageMapper;
        _runtimeConverterService = runtimeConverterService;
        _serviceUniqueName = serviceConfiguration.UniqueName;
    }

    /// <summary>
    /// Creates the asynchronous.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public async ValueTask<ProjectRecord> CreateAsync(string name)
    {
        using ProjectContext context = await _projectContextFactory.CreateDbContextAsync();
        var emptyProjectRecord = new ProjectRecord
        {
            Meta = new ProjectMetaRecord
            {
                Id = Guid.NewGuid(),
                Name = name,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                State = ProjectState.Draft,
                ServiceUniqueName = _serviceUniqueName
            }
        };

        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ProjectRecord> result = await context.AyBorgProjects!.AddAsync(emptyProjectRecord);
        await context.SaveChangesAsync();
        _logger.LogTrace("Created new project [{result.Entity.Meta.Name} | {result.Entity.DbId}].", result.Entity.Meta.Name, result.Entity.DbId);
        if (!context.AyBorgProjects.Any(p => p.Meta.IsActive && p.Meta.ServiceUniqueName == _serviceUniqueName))
        {
            if ((await TryActivateAsync(result.Entity.Meta.DbId, true)).IsSuccessful)
            {
                result.Entity.Meta.IsActive = true;
                _logger.LogTrace("Creating active project.");
            }
        }

        return result.Entity;
    }

    /// <summary>
    /// Deletes asynchronous.
    /// </summary>
    /// <param name="projectId">The project id.</param>
    /// <returns></returns>
    public async ValueTask<ProjectManagementResult> TryDeleteAsync(Guid projectId)
    {
        using ProjectContext context = await _projectContextFactory.CreateDbContextAsync();
        List<ProjectMetaRecord> metas = await context.AyBorgProjectMetas!.Where(pm => pm.Id.Equals(projectId)).ToListAsync();
        if (!metas.Any())
        {
            return new ProjectManagementResult(false, "Project not found.");
        }

        ProjectMetaRecord? activeProjectMeta = metas.FirstOrDefault(pm => pm.IsActive);
        if (activeProjectMeta != null && _engineHost.ActiveProject != null && _engineHost.ActiveProject.Meta.Id.Equals(activeProjectMeta.Id))
        {
            // Need to deactivate the project.
            if (!await _engineHost.TryDeactivateProjectAsync())
            {
                _logger.LogWarning($"Could not deactivate project.");
                return new ProjectManagementResult(false, "Could not deactivate project.");
            }
        }

        List<ProjectRecord> projects = await context.AyBorgProjects!.Where(p => p.Meta.Id.Equals(projectId)).ToListAsync();
        context.AyBorgProjects!.RemoveRange(projects);
        context.AyBorgProjectMetas!.RemoveRange(metas);
        await context.SaveChangesAsync();

        _logger.LogTrace("Removed project  with id [{projectId}].", projectId);
        return new ProjectManagementResult(true, null);
    }

    /// <summary>
    /// Activates asynchronous.
    /// </summary>
    /// <param name="projectMetaId">The project identifier.</param>
    /// <param name="isActive">if set to <c>true</c> [is active].</param>
    /// <returns></returns>
    public async ValueTask<ProjectManagementResult> TryActivateAsync(Guid projectMetaId, bool isActive)
    {
        using ProjectContext context = await _projectContextFactory.CreateDbContextAsync();
        ProjectMetaRecord? orgMetaRecord = await context.AyBorgProjectMetas!.FindAsync(projectMetaId);
        if (orgMetaRecord == null)
        {
            _logger.LogWarning($"No project found to activate.");
            return new ProjectManagementResult(false, "No project found to activate.");
        }

        if (orgMetaRecord.ServiceUniqueName != _serviceUniqueName)
        {
            _logger.LogWarning("Project [{orgMetaRecord.Name}] is not owned by this service.", orgMetaRecord.Name);
            return new ProjectManagementResult(false, "Project is not owned by this service.");
        }

        ProjectMetaRecord? lastActiveMetaRecord = await context.AyBorgProjectMetas!.FirstOrDefaultAsync(x => x.IsActive && x.ServiceUniqueName == _serviceUniqueName);
        if (lastActiveMetaRecord == null)
        {
            _logger.LogTrace("No active project.");
        }
        else
        {
            if (!await _engineHost.TryDeactivateProjectAsync())
            {
                _logger.LogWarning($"Could not deactivate project.");
                return new ProjectManagementResult(false, "Could not deactivate project.");
            }

            lastActiveMetaRecord.IsActive = false;
            _logger.LogTrace("Project [{lastActiveMetaRecord.DbId}] deactivated.", lastActiveMetaRecord.Id);
        }

        // The whole project record need to be loaded and converted to a runtime project.
        if (isActive)
        {
            IQueryable<ProjectRecord> queryProject = CreateFullProjectQuery(context);
            ProjectRecord orgProjectRecord = await queryProject.FirstAsync(x => x.Meta.DbId.Equals(projectMetaId));
            _logger.LogTrace("Loading project [{orgProjectRecord.Meta.Name}] with step count [{orgProjectRecord.Steps.Count}].", orgProjectRecord.Meta.Name, orgProjectRecord.Steps.Count);
            Project project = await _runtimeConverterService.ConvertAsync(orgProjectRecord);
            if (!await _engineHost.TryActivateProjectAsync(project))
            {
                _logger.LogWarning("Could not activate project [{projectMetaDbId}].", projectMetaId);
                return new ProjectManagementResult(false, "Could not activate project.");
            }
        }

        orgMetaRecord.IsActive = isActive;
        if (isActive)
        {
            _logger.LogInformation("Project [{orgMetaRecord.Name}] with id [{orgMetaRecord.DbId}] activated.", orgMetaRecord.Name, orgMetaRecord.Id);
        }
        else
        {
            _logger.LogInformation("Project [{orgMetaRecord.Name}] with id [{orgMetaRecord.DbId}] deactivated.", orgMetaRecord.Name, orgMetaRecord.Id);
        }
        await context.SaveChangesAsync();

        return new ProjectManagementResult(true, null, orgMetaRecord.DbId);
    }

    /// <summary>
    /// Gets all project metas asynchronous.
    /// </summary>
    public async ValueTask<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync()
    {
        using ProjectContext context = await _projectContextFactory.CreateDbContextAsync();
        return await context.AyBorgProjectMetas!.ToListAsync();
    }

    /// <summary>
    /// Gets the project settings asynchronous.
    /// </summary>
    public async ValueTask<ProjectSettingsRecord> GetSettingsAsync(Guid projectMetaDatabaseId)
    {
        using ProjectContext context = await _projectContextFactory.CreateDbContextAsync();
        ProjectRecord? projectRecord = await context.AyBorgProjects!.Include(x => x.Settings).FirstOrDefaultAsync(x => x.Meta.DbId.Equals(projectMetaDatabaseId));
        if (projectRecord == null)
        {
            _logger.LogWarning("No project found with id [{projectDatabaseId}].", projectMetaDatabaseId);
            return null!;
        }

        return projectRecord.Settings;
    }

    /// <summary>
    /// Loads the active project asynchronous.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<ProjectManagementResult> TryLoadActiveAsync()
    {
        using ProjectContext context = await _projectContextFactory.CreateDbContextAsync();
        List<ProjectMetaRecord> projectMetas = await context.AyBorgProjectMetas!.Where(p => p.IsActive).ToListAsync();
        projectMetas = projectMetas.Where(p => p.ServiceUniqueName.Equals(_serviceUniqueName, StringComparison.InvariantCulture)).ToList();
        if (projectMetas.Count > 1)
        {
            _logger.LogWarning("More than one active project found.");
            return new ProjectManagementResult(false, "More than one active project found.");
        }

        ProjectMetaRecord? projectMetaRecord = projectMetas.FirstOrDefault();
        if (projectMetaRecord == null)
        {
            _logger.LogTrace("No active project found.");
            return new ProjectManagementResult(false, "No active project found");
        }

        bool result = (await TryActivateAsync(projectMetaRecord.DbId, true)).IsSuccessful;
        if (!result)
        {
            _logger.LogWarning("Could not activate project.");
            return new ProjectManagementResult(false, "Could not activate project.");
        }

        _logger.LogInformation("Project [{projectMetaRecord.Name}] activated.", projectMetaRecord.Name);
        return new ProjectManagementResult(true, null, projectMetaRecord.DbId);
    }

    /// <summary>
    /// Save active project asynchronous.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<ProjectManagementResult> TrySaveActiveAsync()
    {
        if (_engineHost.ActiveProject == null)
        {
            _logger.LogWarning("No active project.");
            return new ProjectManagementResult(false, "No active project.");
        }

        using ProjectContext context = await _projectContextFactory.CreateDbContextAsync();
        ProjectMetaRecord? previousProjectMetaRecord = await context.AyBorgProjectMetas!.Where(p => p.Id.Equals(_engineHost.ActiveProject.Meta.Id)).FirstOrDefaultAsync(p => p.IsActive);
        if (previousProjectMetaRecord == null)
        {
            _logger.LogWarning($"No project found to save.");
            return new ProjectManagementResult(false, "No project found to save.");
        }

        ProjectRecord projectRecord = _runtimeToStorageMapper.Map(_engineHost.ActiveProject);
        projectRecord.Meta = previousProjectMetaRecord with { DbId = Guid.Empty, UpdatedDate = DateTime.UtcNow };
        projectRecord.Settings.DbId = Guid.Empty;

        try
        {
            await context.AyBorgProjects!.AddAsync(projectRecord);
            await context.AyBorgProjectMetas!.AddAsync(projectRecord.Meta);
            await context.AyBorgProjectSettings!.AddAsync(projectRecord.Settings);
            previousProjectMetaRecord.IsActive = false;
            await context.SaveChangesAsync();
            _engineHost.ActiveProject.Meta.Id = projectRecord.Meta.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not update project [{projectRecord.Meta.Name}].", projectRecord.Meta.Name);
            return new ProjectManagementResult(false, "Could not update project.");
        }

        return new ProjectManagementResult(true, null, projectRecord.Meta.DbId);
    }

    public async ValueTask<ProjectManagementResult> TrySaveNewVersionAsync(Guid projectMetaDbId, ProjectState projectState, string newVersionName, string comment, string? approver = null)
    {
        using ProjectContext context = await _projectContextFactory.CreateDbContextAsync();
        ProjectMetaRecord? previousProjectMetaRecord = await context.AyBorgProjectMetas!.FirstOrDefaultAsync(p => p.DbId.Equals(projectMetaDbId));
        if (previousProjectMetaRecord == null)
        {
            _logger.LogWarning($"No project found to save.");
            return new ProjectManagementResult(false, "No project found to save.");
        }

        // Moving from draft to review state.
        if (previousProjectMetaRecord.State == ProjectState.Draft)
        {
            return await TrySaveDraftToReviewAsync(previousProjectMetaRecord, newVersionName, comment, context);
        }

        if (previousProjectMetaRecord.State == ProjectState.Review && projectState == ProjectState.Draft)
        {
            previousProjectMetaRecord.State = projectState;
            previousProjectMetaRecord.Comment = comment;
            await context.SaveChangesAsync();
            return new ProjectManagementResult(true, null, previousProjectMetaRecord.DbId);
        }

        if (projectState == ProjectState.Ready && string.IsNullOrEmpty(approver))
        {
            _logger.LogWarning("Approver is required.");
            return new ProjectManagementResult(false, "Approver is required.");
        }

        IQueryable<ProjectRecord> queryProject = CreateFullProjectQuery(context);
        ProjectRecord previousProjectRecord = await queryProject.FirstAsync(x => x.Meta.DbId.Equals(projectMetaDbId));

        ProjectMetaRecord projectMetaRecord = previousProjectMetaRecord with
        {
            DbId = Guid.Empty,
            State = projectState,
            VersionName = newVersionName,
            VersionIteration = previousProjectMetaRecord.VersionIteration + 1,
            Comment = comment,
            UpdatedDate = DateTime.UtcNow,
            ApprovedBy = approver
        };

        ProjectSettingsRecord projectSettingsRecord = previousProjectRecord.Settings with
        {
            DbId = Guid.Empty,
            IsForceResultCommunicationEnabled = false,
            IsForceWebUiCommunicationEnabled = false
        };

        ProjectRecord projectRecord = previousProjectRecord with {
            DbId = Guid.Empty,
            Meta = projectMetaRecord,
            Settings = projectSettingsRecord,
            Steps = new(),
            Links = new()
            };

        projectRecord.Steps.Clear();
        projectRecord.Links.Clear();
        foreach(StepRecord s in previousProjectRecord.Steps)
        {
            StepRecord ns = s with { DbId = Guid.Empty, ProjectRecord = projectRecord, ProjectRecordId = projectRecord.DbId, Ports = new() };
            foreach(PortRecord p in s.Ports)
            {
                PortRecord np = p with { DbId = Guid.Empty, StepRecord = ns, StepRecordId = Guid.Empty };
                ns.Ports.Add(np);
            }
            projectRecord.Steps.Add(ns);
        }

        foreach(LinkRecord l in previousProjectRecord.Links)
        {
            LinkRecord nl = l with { DbId = Guid.Empty, ProjectRecord = projectRecord, ProjectRecordId = projectRecord.DbId };
            projectRecord.Links.Add(nl);
        }

        try
        {
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ProjectRecord> result = await context.AyBorgProjects!.AddAsync(projectRecord);
            await context.SaveChangesAsync();
            if(_engineHost.ActiveProject != null && _engineHost.ActiveProject.Meta.Id.Equals(previousProjectMetaRecord.Id))
            {
                _engineHost.ActiveProject.Settings.IsForceResultCommunicationEnabled = false;
                _engineHost.ActiveProject.Settings.IsForceWebUiCommunicationEnabled = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not save project.");
        }
        _logger.LogTrace("Project [{projectRecord.Meta.Name}] saved with id [{projectRecord.Meta.DbId}].", projectMetaRecord.Name, projectMetaRecord.DbId);
        return new ProjectManagementResult(true, null, projectMetaRecord.DbId);
    }

    private async ValueTask<ProjectManagementResult> TrySaveDraftToReviewAsync(ProjectMetaRecord projectMetaRecord, string versionName, string comment, ProjectContext context)
    {
        try
        {
            long previousVersionIteration = projectMetaRecord.VersionIteration;
            projectMetaRecord.State = ProjectState.Review;
            projectMetaRecord.Comment = comment;
            projectMetaRecord.VersionName = versionName;
            projectMetaRecord.VersionIteration++;
            projectMetaRecord.UpdatedDate = DateTime.UtcNow;

            // Remove all drafts from the history.
            IQueryable<ProjectMetaRecord> metaQuery = context.AyBorgProjectMetas!.Where(pm => pm.Id.Equals(projectMetaRecord.Id)
                                                                && pm.State == ProjectState.Draft
                                                                && pm.VersionIteration.Equals(previousVersionIteration)
                                                                && pm.DbId != projectMetaRecord.DbId);
            context.AyBorgProjectMetas!.RemoveRange(metaQuery);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not save project [{projectMetaRecord.Name}] as review.", projectMetaRecord.Name);
            return new ProjectManagementResult(false, "Could not save project as review.", null);
        }

        return new ProjectManagementResult(true, null, projectMetaRecord.DbId);
    }

    private static IQueryable<ProjectRecord> CreateFullProjectQuery(ProjectContext context)
    {
        return context.AyBorgProjects!.Include(x => x.Meta)
                                .Include(x => x.Settings)
                                .Include(x => x.Steps)
                                .ThenInclude(x => x.MetaInfo)
                                .Include(x => x.Steps)
                                .ThenInclude(x => x.Ports)
                                .Include(x => x.Links)
                                .AsSplitQuery();
    }
}

public record struct ProjectManagementResult(bool IsSuccessful, string? Message, Guid? ProjectMetaDbId = null);
