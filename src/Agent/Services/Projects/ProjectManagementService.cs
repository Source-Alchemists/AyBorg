﻿using System.Runtime.CompilerServices;
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
    /// <param name="runtimeToStorageMapper">The runtime to storage mapper.</param>
    /// <param name="runtimeConverterService">The runtime converter service.</param>
    /// <param name="auditProviderService">The audit provider service.</param>
    public ProjectManagementService(ILogger<ProjectManagementService> logger,
                                    IServiceConfiguration serviceConfiguration,
                                    IProjectRepository projectRepository,
                                    IEngineHost runtimeHost,
                                    IRuntimeToStorageMapper runtimeToStorageMapper,
                                    IRuntimeConverterService runtimeConverterService,
                                    IAuditProviderService auditProviderService)
    {
        _logger = logger;
        _projectRepository = projectRepository;
        _engineHost = runtimeHost;
        _runtimeToStorageMapper = runtimeToStorageMapper;
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
            if (activeProjectMeta != null && _engineHost.ActiveProject != null && _engineHost.ActiveProject.Meta.Id.Equals(activeProjectMeta.Id))
            {
                // Need to deactivate the project.
                if (!await _engineHost.TryDeactivateProjectAsync())
                {
                    throw new Exception("Could not deactivate engine project.");
                }
            }

            if (!await _projectRepository.TryDeleteAsync(projectMetaId))
            {
                throw new Exception("Could not delete project.");
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
            ProjectMetaRecord? orgMetaRecord = await _projectRepository.FindMetaAsync(projectMetaDbId);
            if (orgMetaRecord == null)
            {
                throw new KeyNotFoundException("No project found to activate.");
            }

            if (orgMetaRecord.ServiceUniqueName != _serviceUniqueName)
            {
                throw new Exception("Project is not owned by this service.");
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
                    throw new Exception("Could not deactivate engine project.");
                }

                lastActiveMetaRecord.IsActive = false;
                if (!await _projectRepository.TryUpdateAsync(lastActiveMetaRecord))
                {
                    throw new Exception("Could not deactivate project.");
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
                    throw new Exception("Could not activate project.");
                }
            }

            orgMetaRecord.IsActive = isActive;
            if (!await _projectRepository.TryUpdateAsync(orgMetaRecord))
            {
                throw new Exception("Could not change active state.");
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
                        _logger.LogCritical(new EventId((int)EventLogType.ProjectState), "Could not deactivate project '{projectName}'.", pm.Name);
                        continue;
                    }
                }
                throw new Exception("More than one active project found.");
            }

            if (!projectMetas.Any())
            {
                throw new Exception("Failed to load active project.");
            }

            ProjectMetaRecord projectMeta = projectMetas.First();

            bool result = (await TryChangeActivationStateAsync(projectMeta.DbId, true)).IsSuccessful;
            if (!result)
            {
                throw new Exception("Could not activate project.");
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
    /// <returns></returns>
    public async ValueTask<ProjectManagementResult> TrySaveActiveAsync()
    {
        try
        {
            if (_engineHost.ActiveProject == null)
            {
                throw new Exception("No active project.");
            }

            ProjectMetaRecord? previousProjectMetaRecord = (await _projectRepository.GetAllMetasAsync(_serviceUniqueName))!.FirstOrDefault(p => p.IsActive);
            if (previousProjectMetaRecord == null)
            {
                throw new KeyNotFoundException("No project found to save.");
            }

            ProjectRecord projectRecord = _runtimeToStorageMapper.Map(_engineHost.ActiveProject);
            projectRecord.Meta = previousProjectMetaRecord with { DbId = Guid.Empty, IsActive = true };
            projectRecord.Settings.DbId = Guid.Empty;

            if (!await _auditProviderService.TryAddAsync(projectRecord))
            {
                throw new Exception("Could not add audit information.");
            }

            if (!await _projectRepository.TrySave(projectRecord))
            {
                throw new Exception("Could not save project.");
            }

            // Deactivate previous project
            previousProjectMetaRecord.IsActive = false;
            if (!await _projectRepository.TryUpdateAsync(previousProjectMetaRecord))
            {
                throw new Exception("Could not change active state.");
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

    public async ValueTask<ProjectManagementResult> TrySaveNewVersionAsync(Guid projectMetaDbId, ProjectState projectState, string newVersionName, string comment, string? approver = null)
    {
        try
        {
            ProjectMetaRecord? previousProjectMetaRecord = await _projectRepository.FindMetaAsync(projectMetaDbId);
            if (previousProjectMetaRecord == null)
            {
                throw new KeyNotFoundException("No project found to save.");
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
                    throw new Exception("Could not update project.");
                }

                _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Project [{projectMetaRecord.Name}] changed to [Draft].", previousProjectMetaRecord.Name);
                return new ProjectManagementResult(true, null, previousProjectMetaRecord.DbId);
            }

            if (projectState == ProjectState.Ready && string.IsNullOrEmpty(approver))
            {
                throw new Exception("Approver is required.");
            }

            // Moving to ready state
            return await SaveProjectAsReady(projectMetaDbId, projectState, newVersionName, comment, approver, previousProjectMetaRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.ProjectState), ex, "Could not save new project version.");
            return new ProjectManagementResult(false, "Could not save new project version.");
        }
    }

    private async ValueTask<ProjectManagementResult> SaveProjectAsReady(Guid projectMetaDbId, ProjectState projectState, string newVersionName, string comment, string? approver, ProjectMetaRecord previousProjectMetaRecord)
    {
        try
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
                    throw new Exception("Could not deactivate project.");
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

            await _projectRepository.AddAsync(projectRecord);
            if (_engineHost.ActiveProject != null && _engineHost.ActiveProject.Meta.Id.Equals(previousProjectMetaRecord.Id))
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
        try
        {
            long previousVersionIteration = projectMetaRecord.VersionIteration;
            projectMetaRecord.State = ProjectState.Review;
            projectMetaRecord.Comment = comment;
            projectMetaRecord.VersionName = versionName;
            projectMetaRecord.VersionIteration++;
            projectMetaRecord.UpdatedDate = DateTime.UtcNow;

            if (!await _projectRepository.TryUpdateAsync(projectMetaRecord))
            {
                throw new Exception("Could not update project.");
            }

            // Remove all drafts from the history.
            IEnumerable<ProjectMetaRecord> metas = (await _projectRepository.GetAllMetasAsync())
                                                    .Where(pm => !pm.DbId.Equals(projectMetaRecord.DbId)
                                                            && pm.Id.Equals(projectMetaRecord.Id)
                                                            && pm.State.Equals(ProjectState.Draft)
                                                            && pm.VersionIteration.Equals(previousVersionIteration));
            if (!await _projectRepository.TryRemoveRangeAsync(metas))
            {
                throw new Exception("Could not remove drafts from history.");
            }

            _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Project [{projectMetaRecord.Name}] changed to [Review].", projectMetaRecord.Name);
            return new ProjectManagementResult(true, null, projectMetaRecord.DbId);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.ProjectState), ex, "Could not save project as review.");
            return new ProjectManagementResult(false, "Could not save project as review.");
        }
    }
}

public record struct ProjectManagementResult(bool IsSuccessful, string? Message, Guid? ProjectMetaDbId = null);
