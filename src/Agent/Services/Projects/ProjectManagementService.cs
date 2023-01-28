using System.Runtime.CompilerServices;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Configuration;

namespace AyBorg.Agent.Services;

internal sealed class ProjectManagementService : IProjectManagementService
{
    private readonly ILogger<ProjectManagementService> _logger;
    private readonly IProjectRepository _projectRepository;
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
    /// <param name="projectRepository">The project repository.</param>
    /// <param name="runtimeHost">The runtime host.</param>
    /// <param name="runtimeToStorageMapper">The runtime to storage mapper.</param>
    /// <param name="runtimeConverterService">The runtime converter service.</param>
    public ProjectManagementService(ILogger<ProjectManagementService> logger,
                                    IServiceConfiguration serviceConfiguration,
                                    IProjectRepository projectRepository,
                                    IEngineHost runtimeHost,
                                    IRuntimeToStorageMapper runtimeToStorageMapper,
                                    IRuntimeConverterService runtimeConverterService)
    {
        _logger = logger;
        _projectRepository = projectRepository;
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
        ProjectRecord result = await _projectRepository.CreateAsync(name, _serviceUniqueName);
        if (!(await _projectRepository.GetAllMetasAsync(_serviceUniqueName)).Any(pm => pm.IsActive) && (await TryChangeActivationStateAsync(result.Meta.DbId, true)).IsSuccessful)
        {
            result.Meta.IsActive = true;
        }

        return result;
    }

    /// <summary>
    /// Deletes asynchronous.
    /// </summary>
    /// <param name="projectMetaId">The project meta id.</param>
    /// <returns></returns>
    public async ValueTask<ProjectManagementResult> TryDeleteAsync(Guid projectMetaId)
    {
        IEnumerable<ProjectMetaRecord> metas = await _projectRepository.GetAllMetasAsync(projectMetaId);
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
                _logger.LogWarning(new EventId((int)EventLogType.ProjectState), $"Could not deactivate project.");
                return new ProjectManagementResult(false, "Could not deactivate project.");
            }
        }

        if (!await _projectRepository.TryDeleteAsync(projectMetaId))
        {
            return new ProjectManagementResult(false, "Could not delete project.");
        }

        _logger.LogInformation(new EventId((int)EventLogType.ProjectRemoved), "Removed project [{projectName}].", metas.First().Name);
        return new ProjectManagementResult(true, null);
    }

    /// <summary>
    /// Changes the activation state.
    /// </summary>
    /// <param name="projectMetaDbId">The project database identifier.</param>
    /// <param name="isActive">if set to <c>true</c> [is active].</param>
    /// <returns></returns>
    public async ValueTask<ProjectManagementResult> TryChangeActivationStateAsync(Guid projectMetaDbId, bool isActive)
    {
        ProjectMetaRecord? orgMetaRecord = await _projectRepository.FindMetaAsync(projectMetaDbId);
        if (orgMetaRecord == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), $"No project found to activate.");
            return new ProjectManagementResult(false, "No project found to activate.");
        }

        if (orgMetaRecord.ServiceUniqueName != _serviceUniqueName)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Project [{orgMetaRecord.Name}] is not owned by this service.", orgMetaRecord.Name);
            return new ProjectManagementResult(false, "Project is not owned by this service.");
        }

        ProjectMetaRecord? lastActiveMetaRecord = (await _projectRepository.GetAllMetasAsync(_serviceUniqueName))!.FirstOrDefault(x => x.IsActive);
        if (lastActiveMetaRecord == null)
        {
            _logger.LogTrace(new EventId((int)EventLogType.ProjectState), "No active project.");
        }
        else
        {
            if (!await _engineHost.TryDeactivateProjectAsync())
            {
                _logger.LogWarning(new EventId((int)EventLogType.ProjectState), $"Could not deactivate project.");
                return new ProjectManagementResult(false, "Could not deactivate project.");
            }

            lastActiveMetaRecord.IsActive = false;
            if (!await _projectRepository.TryUpdateAsync(lastActiveMetaRecord))
            {
                _logger.LogWarning(new EventId((int)EventLogType.ProjectState), $"Could not deactivate project.");
                return new ProjectManagementResult(false, "Could not deactivate project.");
            }

            _logger.LogTrace(new EventId((int)EventLogType.ProjectState), "Project [{lastActiveMetaRecord.DbId}] deactivated.", lastActiveMetaRecord.Id);
        }

        // The whole project record need to be loaded and converted to a runtime project.
        if (isActive)
        {
            ProjectRecord orgProjectRecord = await _projectRepository.FindAsync(projectMetaDbId);
            _logger.LogTrace(new EventId((int)EventLogType.ProjectState), "Loading project [{orgProjectRecord.Meta.Name}] with step count [{orgProjectRecord.Steps.Count}].", orgProjectRecord.Meta.Name, orgProjectRecord.Steps.Count);
            Project project = await _runtimeConverterService.ConvertAsync(orgProjectRecord);
            if (!await _engineHost.TryActivateProjectAsync(project))
            {
                _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Could not activate project [{projectMetaDbId}].", projectMetaDbId);
                return new ProjectManagementResult(false, "Could not activate project.");
            }
        }

        orgMetaRecord.IsActive = isActive;
        if (!await _projectRepository.TryUpdateAsync(orgMetaRecord))
        {
            return new ProjectManagementResult(false, "Could not change active state.");
        }

        _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Project [{orgMetaRecord.Name}] activated.", orgMetaRecord.Name);

        return new ProjectManagementResult(true, null, orgMetaRecord.DbId);
    }

    /// <summary>
    /// Gets all project metas asynchronous.
    /// </summary>
    public ValueTask<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync() => _projectRepository.GetAllMetasAsync();

    /// <summary>
    /// Loads the active project asynchronous.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<ProjectManagementResult> TryLoadActiveAsync()
    {
        IEnumerable<ProjectMetaRecord> projectMetas = (await _projectRepository.GetAllMetasAsync(_serviceUniqueName))!.Where(p => p.IsActive);
        // More then one active project. Deactivating each.
        if (projectMetas.Count() > 1)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "More than one active project found. Deactivating each ...");
            foreach (ProjectMetaRecord pm in projectMetas)
            {
                pm.IsActive = false;
                if (!await _projectRepository.TryUpdateAsync(pm))
                {
                    _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Could not deactivate project '{projectName}'.", pm.Name);
                    continue;
                }
            }
            return new ProjectManagementResult(false, "More than one active project found.");
        }

        if (!projectMetas.Any())
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Failed to load active project.");
            return new ProjectManagementResult(false, "Failed to load active project.");
        }

        ProjectMetaRecord projectMeta = projectMetas.First();

        bool result = (await TryChangeActivationStateAsync(projectMeta.DbId, true)).IsSuccessful;
        if (!result)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Could not activate project.");
            return new ProjectManagementResult(false, "Could not activate project.");
        }

        return new ProjectManagementResult(true, null, projectMeta.DbId);
    }

    /// <summary>
    /// Save active project asynchronous.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<ProjectManagementResult> TrySaveActiveAsync()
    {
        if (_engineHost.ActiveProject == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project.");
            return new ProjectManagementResult(false, "No active project.");
        }

        ProjectMetaRecord? previousProjectMetaRecord = (await _projectRepository.GetAllMetasAsync(_serviceUniqueName))!.FirstOrDefault(p => p.IsActive);
        if (previousProjectMetaRecord == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), $"No project found to save.");
            return new ProjectManagementResult(false, "No project found to save.");
        }

        previousProjectMetaRecord.IsActive = false;
        if (!await _projectRepository.TryUpdateAsync(previousProjectMetaRecord))
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Could not change active state.");
            return new ProjectManagementResult(false, "Could not change active state.");
        }

        ProjectRecord projectRecord = _runtimeToStorageMapper.Map(_engineHost.ActiveProject);
        projectRecord.Meta = previousProjectMetaRecord with { DbId = Guid.Empty, IsActive = true };
        projectRecord.Settings.DbId = Guid.Empty;

        if (!await _projectRepository.TrySave(projectRecord))
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Could not save project.");
            return new ProjectManagementResult(false, "Could not save project.");
        }

        _logger.LogInformation(new EventId((int)EventLogType.ProjectSaved), "Project [{projectRecord.Meta.Name}] saved.", projectRecord.Meta.Name);

        _engineHost.ActiveProject.Meta.Id = projectRecord.Meta.Id;
        return new ProjectManagementResult(true, null, projectRecord.Meta.DbId);
    }

    public async ValueTask<ProjectManagementResult> TrySaveNewVersionAsync(Guid projectMetaDbId, ProjectState projectState, string newVersionName, string comment, string? approver = null)
    {
        ProjectMetaRecord? previousProjectMetaRecord = await _projectRepository.FindMetaAsync(projectMetaDbId);
        if (previousProjectMetaRecord == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), $"No project found to save.");
            return new ProjectManagementResult(false, "No project found to save.");
        }

        // Moving from draft to review state
        if (previousProjectMetaRecord.State == ProjectState.Draft)
        {
            return await TrySaveDraftToReviewAsync(previousProjectMetaRecord, newVersionName, comment);
        }

        // Moving back to draft state
        if (previousProjectMetaRecord.State == ProjectState.Review && projectState == ProjectState.Draft)
        {
            previousProjectMetaRecord.State = projectState;
            previousProjectMetaRecord.Comment = comment;
            if (!await _projectRepository.TryUpdateAsync(previousProjectMetaRecord))
            {
                _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Could not update project.");
                return new ProjectManagementResult(false, "Could not update project.");
            }

            _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Project [{projectMetaRecord.Name}] changed to [Draft].", previousProjectMetaRecord.Name);
            return new ProjectManagementResult(true, null, previousProjectMetaRecord.DbId);
        }

        if (projectState == ProjectState.Ready && string.IsNullOrEmpty(approver))
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Approver is required.");
            return new ProjectManagementResult(false, "Approver is required.");
        }

        // Moving to ready state
        return await SaveProjectAsReady(projectMetaDbId, projectState, newVersionName, comment, approver, previousProjectMetaRecord);
    }

    private async ValueTask<ProjectManagementResult> SaveProjectAsReady(Guid projectMetaDbId, ProjectState projectState, string newVersionName, string comment, string? approver, ProjectMetaRecord previousProjectMetaRecord)
    {
        ProjectRecord previousProjectRecord = await _projectRepository.FindAsync(projectMetaDbId);

        ProjectMetaRecord projectMetaRecord = previousProjectMetaRecord with
        {
            DbId = Guid.NewGuid(),
            State = projectState,
            VersionName = newVersionName,
            VersionIteration = previousProjectMetaRecord.VersionIteration + 1,
            Comment = comment,
            UpdatedDate = DateTime.UtcNow,
            ApprovedBy = approver
        };

        if (previousProjectRecord.Meta.IsActive)
        {
            previousProjectRecord.Meta.IsActive = false;
            if (!await _projectRepository.TryUpdateAsync(previousProjectRecord.Meta))
            {
                _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Could not deactivate project. Stopping action");
                return new ProjectManagementResult(false, "Could not deactivate project.");
            }
        }

        ProjectSettingsRecord projectSettingsRecord = previousProjectRecord.Settings with
        {
            DbId = Guid.Empty,
            IsForceResultCommunicationEnabled = false
        };

        ProjectRecord projectRecord = previousProjectRecord with
        {
            DbId = Guid.Empty,
            Meta = projectMetaRecord,
            Settings = projectSettingsRecord,
            Steps = new(),
            Links = new()
        };

        ReconstructSteps(previousProjectRecord, projectRecord);

        try
        {
            await _projectRepository.AddAsync(projectRecord);
            if (_engineHost.ActiveProject != null && _engineHost.ActiveProject.Meta.Id.Equals(previousProjectMetaRecord.Id))
            {
                _engineHost.ActiveProject.Settings.IsForceResultCommunicationEnabled = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), ex, "Could not save project.");
        }

        _logger.LogInformation("Project [{projectName}] saved as ready.", projectMetaRecord.Name);
        return new ProjectManagementResult(true, null, projectMetaRecord.DbId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReconstructSteps(ProjectRecord previousProjectRecord, ProjectRecord projectRecord)
    {
        projectRecord.Steps.Clear();
        projectRecord.Links.Clear();
        foreach (StepRecord s in previousProjectRecord.Steps)
        {
            StepRecord ns = s with
            {
                DbId = Guid.Empty,
                ProjectRecord = projectRecord,
                ProjectRecordId = projectRecord.DbId,
                MetaInfo = s.MetaInfo with { DbId = Guid.Empty },
                Ports = new()
            };
            foreach (PortRecord p in s.Ports)
            {
                PortRecord np = p with
                {
                    DbId = Guid.Empty,
                    StepRecord = ns,
                    StepRecordId = Guid.Empty
                };
                ns.Ports.Add(np);
            }
            projectRecord.Steps.Add(ns);
        }

        foreach (LinkRecord l in previousProjectRecord.Links)
        {
            LinkRecord nl = l with
            {
                DbId = Guid.Empty,
                ProjectRecord = projectRecord,
                ProjectRecordId = projectRecord.DbId
            };
            projectRecord.Links.Add(nl);
        }
    }

    private async ValueTask<ProjectManagementResult> TrySaveDraftToReviewAsync(ProjectMetaRecord projectMetaRecord, string versionName, string comment)
    {
        long previousVersionIteration = projectMetaRecord.VersionIteration;
        projectMetaRecord.State = ProjectState.Review;
        projectMetaRecord.Comment = comment;
        projectMetaRecord.VersionName = versionName;
        projectMetaRecord.VersionIteration++;
        projectMetaRecord.UpdatedDate = DateTime.UtcNow;

        if (!await _projectRepository.TryUpdateAsync(projectMetaRecord))
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Could not update project.");
            return new ProjectManagementResult(false, "Could not update project.");
        }

        // Remove all drafts from the history.
        IEnumerable<ProjectMetaRecord> metas = (await _projectRepository.GetAllMetasAsync())
                                                .Where(pm => !pm.DbId.Equals(projectMetaRecord.DbId)
                                                        && pm.Id.Equals(projectMetaRecord.Id)
                                                        && pm.State.Equals(ProjectState.Draft)
                                                        && pm.VersionIteration.Equals(previousVersionIteration));
        if (!await _projectRepository.TryRemoveRangeAsync(metas))
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Could not remove drafts from history.");
            return new ProjectManagementResult(false, "Could not remove drafts from history.");
        }

        _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Project [{projectMetaRecord.Name}] changed to [Review].", projectMetaRecord.Name);
        return new ProjectManagementResult(true, null, projectMetaRecord.DbId);
    }
}

public record struct ProjectManagementResult(bool IsSuccessful, string? Message, Guid? ProjectMetaDbId = null);
