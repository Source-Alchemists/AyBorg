using Microsoft.EntityFrameworkCore;
using Atomy.Database.Data;
using Atomy.SDK.Data.DAL;
using Atomy.SDK.Data.Mapper;
using Atomy.SDK.Projects;

namespace Atomy.Agent.Services;

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
    /// <param name="configuration">The configuration.</param>
    /// <param name="projectContextFactory">The project context.</param>
    /// <param name="runtimeHost">The runtime host.</param>
    /// <param name="runtimeToStorageMapper">The runtime to storage mapper.</param>
    /// <param name="runtimeConverterService">The runtime converter service.</param>
    public ProjectManagementService(ILogger<ProjectManagementService> logger,
                                    IConfiguration configuration,
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

        _serviceUniqueName = configuration.GetValue<string>("Atomy:Service:UniqueName");
    }

    /// <summary>
    /// Creates the asynchronous.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public async Task<ProjectRecord> CreateAsync(string name)
    {
        using var context = await _projectContextFactory.CreateDbContextAsync();
        var emptyProjectRecord = new ProjectRecord
        {
            Meta = new ProjectMetaRecord
            {
                Name = name,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                State = ProjectState.Draft,
                ServiceUniqueName = _serviceUniqueName
            }
        };

        var result = await context.AtomyProjects!.AddAsync(emptyProjectRecord);
        await context.SaveChangesAsync();
        _logger.LogTrace("Created new project [{result.Entity.Meta.Name} | {result.Entity.DbId}].", result.Entity.Meta.Name, result.Entity.DbId);
        if (!context.AtomyProjects.Any(p => p.Meta.IsActive && p.Meta.ServiceUniqueName == _serviceUniqueName))
        {
            if (await TryActivateAsync(result.Entity.Meta.DbId, true))
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
    public async Task<bool> TryDeleteAsync(Guid projectId)
    {
        using var context = await _projectContextFactory.CreateDbContextAsync();
        var metas = context.AtomyProjectMetas!.ToList();
        var orgMetaRecord = await context.AtomyProjectMetas!.FindAsync(projectId);
        if (orgMetaRecord == null)
        {
            _logger.LogWarning($"No project found to delete.");
            return false;
        }

        if (_engineHost.ActiveProject != null && _engineHost.ActiveProject.Meta.Id.Equals(orgMetaRecord.DbId))
        {
            // Need to deactivate the project.
            if (!await _engineHost.TryDeactivateProjectAsync())
            {
                _logger.LogWarning($"Could not deactivate project.");
                return false;
            }
        }

        var orgProjectRecord = await context.AtomyProjects!.FirstAsync(x => x.Meta.DbId.Equals(projectId));
        context.AtomyProjects!.Remove(orgProjectRecord);
        await context.SaveChangesAsync();

        _logger.LogTrace("Removed project [{orgMetaRecord.Name}] with id [{orgMetaRecord.DbId}].", orgMetaRecord.Name, orgMetaRecord.DbId);
        return true;
    }

    /// <summary>
    /// Activates asynchronous.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="isActive">if set to <c>true</c> [is active].</param>
    /// <returns></returns>
    public async Task<bool> TryActivateAsync(Guid projectId, bool isActive)
    {
        using var context = await _projectContextFactory.CreateDbContextAsync();
        var orgMetaRecord = await context.AtomyProjectMetas!.FindAsync(projectId);
        if (orgMetaRecord == null)
        {
            _logger.LogWarning($"No project found to activate.");
            return false;
        }

        if(orgMetaRecord.ServiceUniqueName != _serviceUniqueName)
        {
            _logger.LogWarning("Project [{orgMetaRecord.Name}] is not owned by this service.", orgMetaRecord.Name);
            return false;
        }

        var lastActiveMetaRecord = await context.AtomyProjectMetas.FirstOrDefaultAsync(x => x.IsActive && x.ServiceUniqueName == _serviceUniqueName);
        if (lastActiveMetaRecord == null)
        {
            _logger.LogTrace("No active project.");
        }
        else
        {
            if (!await _engineHost.TryDeactivateProjectAsync())
            {
                _logger.LogWarning($"Could not deactivate project.");
                return false;
            }

            lastActiveMetaRecord.IsActive = false;
            _logger.LogTrace("Project [{lastActiveMetaRecord.DbId}] deactivated.", lastActiveMetaRecord.DbId);
        }

        // The whole project record need to be loaded and converted to a runtime project.
        if (isActive)
        {
            IQueryable<ProjectRecord> queryProject = CreateFullProjectQuery(context);
            var orgProjectRecord = await queryProject.FirstAsync(x => x.Meta.DbId.Equals(projectId));
            _logger.LogTrace("Loading project [{orgProjectRecord.Meta.Name}] with step count [{orgProjectRecord.Steps.Count}].", orgProjectRecord.Meta.Name, orgProjectRecord.Steps.Count);
            var project = await _runtimeConverterService.ConvertAsync(orgProjectRecord);
            if (!await _engineHost.TryActivateProjectAsync(project))
            {
                _logger.LogWarning("Could not activate project [{projectId}].", projectId);
                return false;
            }
        }

        orgMetaRecord.IsActive = isActive;
        if(isActive)
        {
            _logger.LogInformation("Project [{orgMetaRecord.Name}] with id [{orgMetaRecord.DbId}] activated.", orgMetaRecord.Name, orgMetaRecord.DbId);
        } else
        {
            _logger.LogInformation("Project [{orgMetaRecord.Name}] with id [{orgMetaRecord.DbId}] deactivated.", orgMetaRecord.Name, orgMetaRecord.DbId);
        }
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Change project state asynchronous.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="state">The state.</param>
    /// <returns></returns>
    public async Task<bool> TryChangeProjectStateAsync(Guid projectId, ProjectState state)
    {
        using var context = await _projectContextFactory.CreateDbContextAsync();
        var orgMetaRecord = await context.AtomyProjectMetas!.FindAsync(projectId);
        if (orgMetaRecord == null)
        {
            _logger.LogWarning($"No project for state change found.");
            return false;
        }

        if(orgMetaRecord.ServiceUniqueName != _serviceUniqueName)
        {
            _logger.LogWarning("Project [{orgMetaRecord.Name}] is not owned by this service.", orgMetaRecord.Name);
            return false;
        }

        if (orgMetaRecord.State == state)
        {
            _logger.LogWarning("Project is already in state [{state}].", state);
            return false;
        }

        if (orgMetaRecord.State == ProjectState.Ready)
        {
            _logger.LogWarning("Project is already in state [{state}]. Cannot change state from [{orgMetaRecord.State}] to [{state}].", state, orgMetaRecord.State, state);
            return false;
        }

        orgMetaRecord.State = state;
        orgMetaRecord.UpdatedDate = DateTime.UtcNow;
        _logger.LogTrace("Project [{orgMetaRecord.DbId}] set state to [{state}].", orgMetaRecord.DbId, state);
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets all project metas asynchronous.
    /// </summary>
    public async Task<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync()
    {
        using var context = await _projectContextFactory.CreateDbContextAsync();
        return await context.AtomyProjectMetas!.ToListAsync();
    }

    /// <summary>
    /// Loads the active project asynchronous.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> TryLoadActiveProjectAsync()
    {
        using var context = await _projectContextFactory.CreateDbContextAsync();
        var projectMetaRecord = await context.AtomyProjectMetas!.FirstOrDefaultAsync(p => p.IsActive && p.ServiceUniqueName == _serviceUniqueName);
        if (projectMetaRecord == null)
        {
            _logger.LogWarning("No active project.");
            return false;
        }

        return await TryActivateAsync(projectMetaRecord.DbId, true);
    }

    /// <summary>
    /// Save active project asynchronous.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> TrySaveActiveProjectAsync()
    {
        if (_engineHost.ActiveProject == null)
        {
            _logger.LogWarning("No active project.");
            return false;
        }

        using var context = await _projectContextFactory.CreateDbContextAsync();
        var projectMetaRecord = await context.AtomyProjectMetas!.FirstOrDefaultAsync(p => p.DbId.Equals(_engineHost.ActiveProject.Meta.Id));
        if (projectMetaRecord == null)
        {
            _logger.LogWarning($"No project found to save.");
            return false;
        }

        var databaseProjectRecord = await context.AtomyProjects!.Include(x => x.Steps)
                                                            .ThenInclude(x => x.Ports)
                                                            .Include(x => x.Links)
                                                            .AsSplitQuery()
                                                            .FirstOrDefaultAsync(p => p.Meta.DbId.Equals(projectMetaRecord.DbId));
        if (databaseProjectRecord == null)
        {
            _logger.LogWarning($"No database project found to save.");
            return false;
        }

        var projectRecord = _runtimeToStorageMapper.Map(_engineHost.ActiveProject);
        projectRecord.DbId = databaseProjectRecord.DbId;
        projectRecord.Meta = databaseProjectRecord.Meta;

        try
        {
            // Always if the projects is be saved, the state is set to draft.
            // The ready state must be set explicitly.
            databaseProjectRecord.Meta.State = ProjectState.Draft;
            databaseProjectRecord.Meta.UpdatedDate = DateTime.UtcNow;
            // Need to update all database identifiers.
            UpdateStepRecordsByDatabaseContext(projectRecord, databaseProjectRecord);
            databaseProjectRecord.Steps = projectRecord.Steps;
            UpdateLinkRecordsByDatabaseContext(projectRecord, databaseProjectRecord);
            databaseProjectRecord.Links = projectRecord.Links;
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not update project [{projectRecord.Meta.Name}].", projectRecord.Meta.Name);
            return false;
        }

        return true;
    }

    private void UpdateStepRecordsByDatabaseContext(ProjectRecord projectRecord, ProjectRecord databaseProjectRecord)
    {
        foreach (var step in projectRecord.Steps)
        {
            if (!databaseProjectRecord.Steps.Any(x => x.Id.Equals(step.Id)))
            {
                _logger.LogTrace("Adding step [{step.Id}] to project [{projectRecord.Meta.Name}] with id [{projectRecord.DbId}].", step.Id, projectRecord.Meta.Name, projectRecord.DbId);
            }
            else
            {
                step.DbId = databaseProjectRecord.Steps.First(x => x.Id.Equals(step.Id)).DbId;
            }
        }
    }

    private void UpdateLinkRecordsByDatabaseContext(ProjectRecord projectRecord, ProjectRecord databaseProjectRecord)
    {
        foreach (var link in projectRecord.Links)
        {
            if (!databaseProjectRecord.Links.Any(x => x.Id.Equals(link.Id)))
            {
                _logger.LogTrace("Adding link [{link.Id}] to project [{projectRecord.Meta.Name}] with id [{projectRecord.DbId}].", link.Id, projectRecord.Meta.Name, projectRecord.DbId);
            }
            else
            {
                link.DbId = databaseProjectRecord.Links.First(x => x.Id.Equals(link.Id)).DbId;
            }
        }
    }

    private static IQueryable<ProjectRecord> CreateFullProjectQuery(ProjectContext context)
    {
        return context.AtomyProjects!.Include(x => x.Meta)
                                .Include(x => x.Steps)
                                .ThenInclude(x => x.MetaInfo)
                                .Include(x => x.Steps)
                                .ThenInclude(x => x.Ports)
                                .Include(x => x.Links)
                                .AsSplitQuery();
    }
}
