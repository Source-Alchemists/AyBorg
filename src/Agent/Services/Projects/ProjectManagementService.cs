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
    private readonly IFlowToStorageMapper _flowToStorageMapper;
    private readonly IRuntimeConverterService _runtimeConverterService;
    private readonly IAuditProviderService _auditProviderService;
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
    /// <param name="flowToStorageMapper">The flow to storage mapper.</param>
    /// <param name="runtimeConverterService">The runtime converter service.</param>
    /// <param name="auditProviderService">The audit provider service.</param>
    public ProjectManagementService(ILogger<ProjectManagementService> logger,
                                    IServiceConfiguration serviceConfiguration,
                                    IProjectRepository projectRepository,
                                    IEngineHost runtimeHost,
                                    IFlowToStorageMapper flowToStorageMapper,
                                    IRuntimeConverterService runtimeConverterService,
                                    IAuditProviderService auditProviderService)
    {
        _logger = logger;
        _projectRepository = projectRepository;
        _engineHost = runtimeHost;
        _flowToStorageMapper = flowToStorageMapper;
        _runtimeConverterService = runtimeConverterService;
        _auditProviderService = auditProviderService;

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
        try
        {
            IEnumerable<ProjectMetaRecord> metas = await _projectRepository.GetAllMetasAsync(projectMetaId);
            if (!metas.Any())
            {
                throw new KeyNotFoundException("Project not found.");
            }

            ProjectMetaRecord? activeProjectMeta = metas.FirstOrDefault(pm => pm.IsActive);
            if (activeProjectMeta != null
                && _engineHost.ActiveProject != null
                && _engineHost.ActiveProject.Meta.Id.Equals(activeProjectMeta.Id)
                && !await _engineHost.TryDeactivateProjectAsync()) // Need to deactivated project.
            {
                throw new ProjectException("Could not deactivate engine project.");
            }

            if (!await _projectRepository.TryDeleteAsync(projectMetaId))
            {
                throw new ProjectException("Could not delete project.");
            }

            _logger.LogInformation(new EventId((int)EventLogType.ProjectRemoved), "Removed project [{projectName}].", metas.First().Name);
            return new ProjectManagementResult(true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.ProjectState), ex, "Could not delete project.");
            return new ProjectManagementResult(false, ex.Message);
        }
    }

    /// <summary>
    /// Changes the activation state.
    /// </summary>
    /// <param name="projectMetaDbId">The project database identifier.</param>
    /// <param name="isActive">if set to <c>true</c> [is active].</param>
    /// <returns></returns>
    public async ValueTask<ProjectManagementResult> TryChangeActivationStateAsync(Guid projectMetaDbId, bool isActive)
    {
        try
        {
            ProjectMetaRecord? orgMetaRecord = await _projectRepository.FindMetaAsync(projectMetaDbId) ?? throw new KeyNotFoundException("No project found to activate.");
            if (orgMetaRecord.ServiceUniqueName != _serviceUniqueName)
            {
                throw new ProjectException("Project is not owned by this service.");
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
                    throw new ProjectException("Could not deactivate engine project.");
                }

                lastActiveMetaRecord.IsActive = false;
                if (!await _projectRepository.TryUpdateAsync(lastActiveMetaRecord))
                {
                    throw new ProjectException("Could not deactivate project.");
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
                    throw new ProjectException("Could not activate project.");
                }
            }

            orgMetaRecord.IsActive = isActive;
            if (!await _projectRepository.TryUpdateAsync(orgMetaRecord))
            {
                throw new ProjectException("Could not change active state.");
            }

            _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Project [{orgMetaRecord.Name}] activated.", orgMetaRecord.Name);
            return new ProjectManagementResult(true, null, orgMetaRecord.DbId);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.ProjectState), ex, "Could not change active state.");
            return new ProjectManagementResult(false, "Could not change active state.");
        }
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
        try
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
                        _logger.LogError(new EventId((int)EventLogType.ProjectState), "Could not deactivate project '{projectName}'.", pm.Name);
                        continue;
                    }
                }
                throw new ProjectException("More than one active project found.");
            }

            if (!projectMetas.Any())
            {
                _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project found.");
                return new ProjectManagementResult(false, "No active project found.");
            }

            ProjectMetaRecord projectMeta = projectMetas.First();

            bool result = (await TryChangeActivationStateAsync(projectMeta.DbId, true)).IsSuccessful;
            if (!result)
            {
                throw new ProjectException("Could not activate project.");
            }

            return new ProjectManagementResult(true, null, projectMeta.DbId);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.ProjectState), ex, "Could not load active project.");
            return new ProjectManagementResult(false, "Could not load active project.");
        }
    }

    /// <summary>
    /// Save active project asynchronous.
    /// </summary>
    /// <param name="userName">Name of the user saving the project.</param>
    /// <returns></returns>
    public async ValueTask<ProjectManagementResult> TrySaveActiveAsync(string userName)
    {
        try
        {
            if (_engineHost.ActiveProject == null)
            {
                throw new ProjectException("No active project.");
            }

            ProjectMetaRecord? previousProjectMetaRecord = (await _projectRepository.GetAllMetasAsync(_serviceUniqueName))!.FirstOrDefault(p => p.IsActive) ?? throw new KeyNotFoundException("No project found to save.");
            ProjectRecord projectRecord = _flowToStorageMapper.Map(_engineHost.ActiveProject);
            projectRecord.Meta = previousProjectMetaRecord with { DbId = Guid.Empty, IsActive = true };
            projectRecord.Settings.DbId = Guid.Empty;

            Guid auditToken = await _auditProviderService.AddAsync(projectRecord, userName);
            if (auditToken.Equals(Guid.Empty))
            {
                throw new ProjectException("Could not add audit information.");
            }

            if (!await _projectRepository.TrySave(projectRecord))
            {
                if (!await _auditProviderService.TryInvalidateAsync(auditToken))
                {
                    throw new ProjectException("Could not invalidate audit information.");
                }

                throw new ProjectException("Could not save project.");
            }

            // Deactivate previous project
            previousProjectMetaRecord.IsActive = false;
            if (!await _projectRepository.TryUpdateAsync(previousProjectMetaRecord))
            {
                throw new ProjectException("Could not change active state.");
            }

            _logger.LogInformation(new EventId((int)EventLogType.ProjectSaved), "Project [{projectRecord.Meta.Name}] saved.", projectRecord.Meta.Name);
            _engineHost.ActiveProject.Meta.Id = projectRecord.Meta.Id;
            return new ProjectManagementResult(true, null, projectRecord.Meta.DbId);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.ProjectState), ex, "Could not save project.");
            return new ProjectManagementResult(false, "Could not save project.");
        }
    }

    public async ValueTask<ProjectManagementResult> TrySaveAsync(Guid projectMetaDbId, ProjectState projectState, string newVersionName, string approver, string comment)
    {
        try
        {
            var informations = new Informations(approver!, approver!, comment, newVersionName);
            ProjectRecord previousProjectRecord = await _projectRepository.FindAsync(projectMetaDbId) ?? throw new KeyNotFoundException("No project found to save.");

            // Moving from draft to review state
            if (previousProjectRecord.Meta.State == ProjectState.Draft)
            {
                return await TrySaveDraftToReviewAsync(previousProjectRecord, informations);
            }

            // Moving back to draft state
            if (previousProjectRecord.Meta.State == ProjectState.Review && projectState == ProjectState.Draft)
            {
                return await TrySaveReviewAsDraftAsync(previousProjectRecord, projectState, informations);
            }

            if (projectState == ProjectState.Ready && string.IsNullOrEmpty(approver))
            {
                throw new ProjectException("Approver is required.");
            }

            // Moving to ready state
            return await TrySaveReviewAsReadyAsync(previousProjectRecord, informations);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.ProjectState), ex, "Could not save new project version.");
            return new ProjectManagementResult(false, "Could not save new project version.");
        }
    }

    private async ValueTask<ProjectManagementResult> TrySaveDraftToReviewAsync(ProjectRecord projectRecord, Informations informations)
    {
        try
        {
            long previousVersionIteration = projectRecord.Meta.VersionIteration;
            projectRecord.Meta.State = ProjectState.Review;
            projectRecord.Meta.Comment = informations.Comment;
            projectRecord.Meta.VersionName = informations.VersionName;
            projectRecord.Meta.VersionIteration++;
            projectRecord.Meta.UpdatedDate = DateTime.UtcNow;

            Guid auditToken = await _auditProviderService.AddAsync(projectRecord, informations.User);
            if (auditToken.Equals(Guid.Empty))
            {
                throw new ProjectException("Could not add audit information.");
            }

            if (!await _projectRepository.TryUpdateAsync(projectRecord.Meta))
            {
                if (!await _auditProviderService.TryInvalidateAsync(auditToken))
                {
                    throw new ProjectException("Could not invalidate audit information.");
                }

                throw new ProjectException("Could not save project.");
            }

            // Remove all drafts from the history.
            IEnumerable<ProjectMetaRecord> metas = (await _projectRepository.GetAllMetasAsync())
                                                    .Where(pm => !pm.DbId.Equals(projectRecord.Meta.DbId)
                                                            && pm.Id.Equals(projectRecord.Meta.Id)
                                                            && pm.State.Equals(ProjectState.Draft)
                                                            && pm.VersionIteration.Equals(previousVersionIteration));
            if (!await _projectRepository.TryRemoveRangeAsync(metas))
            {
                throw new ProjectException("Could not remove drafts from history.");
            }

            _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Project [{projectMetaRecord.Name}] changed to [Review].", projectRecord.Meta.Name);
            return new ProjectManagementResult(true, null, projectRecord.Meta.DbId);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.ProjectState), ex, "Could not save project as review.");
            return new ProjectManagementResult(false, "Could not save project as review.");
        }
    }

    private async ValueTask<ProjectManagementResult> TrySaveReviewAsDraftAsync(ProjectRecord projectRecord, ProjectState projectState, Informations informations)
    {
        projectRecord.Meta.State = projectState;
        projectRecord.Meta.Comment = informations.Comment;
        Guid auditToken = await _auditProviderService.AddAsync(projectRecord, informations.User);

        if (auditToken.Equals(Guid.Empty))
        {
            throw new ProjectException("Could not add audit information.");
        }

        if (!await _projectRepository.TryUpdateAsync(projectRecord.Meta))
        {
            if (!await _auditProviderService.TryInvalidateAsync(auditToken))
            {
                throw new ProjectException("Could not invalidate audit information.");
            }
            throw new ProjectException("Could not update project.");
        }

        _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Project [{projectMetaRecord.Name}] changed to [Draft].", projectRecord.Meta.Name);
        return new ProjectManagementResult(true, null, projectRecord.Meta.DbId);
    }

    private async ValueTask<ProjectManagementResult> TrySaveReviewAsReadyAsync(ProjectRecord previousProjectRecord, Informations informations)
    {
        try
        {
            ProjectMetaRecord projectMetaRecord = previousProjectRecord.Meta with
            {
                DbId = Guid.NewGuid(),
                State = ProjectState.Ready,
                VersionName = informations.VersionName,
                VersionIteration = previousProjectRecord.Meta.VersionIteration + 1,
                Comment = informations.Comment,
                UpdatedDate = DateTime.UtcNow,
                ApprovedBy = informations.Approver
            };

            if (previousProjectRecord.Meta.IsActive)
            {
                previousProjectRecord.Meta.IsActive = false;
                if (!await _projectRepository.TryUpdateAsync(previousProjectRecord.Meta))
                {
                    throw new ProjectException("Could not deactivate project.");
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

            Guid auditToken = await _auditProviderService.AddAsync(projectRecord, informations.Approver);
            if (auditToken.Equals(Guid.Empty))
            {
                throw new ProjectException("Could not add audit information.");
            }

            if (await _projectRepository.AddAsync(projectRecord) == null)
            {
                if (!await _auditProviderService.TryInvalidateAsync(auditToken))
                {
                    throw new ProjectException("Could not invalidate audit information.");
                }

                throw new ProjectException("Could not save project.");
            }

            if (_engineHost.ActiveProject != null && _engineHost.ActiveProject.Meta.Id.Equals(previousProjectRecord.Meta.Id))
            {
                _engineHost.ActiveProject.Settings.IsForceResultCommunicationEnabled = false;
            }

            _logger.LogInformation("Project [{projectName}] saved as ready.", projectMetaRecord.Name);
            return new ProjectManagementResult(true, null, projectMetaRecord.DbId);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.ProjectState), ex, "Could not save project as ready.");
            return new ProjectManagementResult(false, "Could not save project as ready.");
        }
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
            foreach (StepPortRecord p in s.Ports)
            {
                StepPortRecord np = p with
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

    private sealed record Informations(string User, string Approver, string Comment, string VersionName);
}

public record struct ProjectManagementResult(bool IsSuccessful, string? Message, Guid? ProjectMetaDbId = null);
