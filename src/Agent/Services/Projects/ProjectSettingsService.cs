/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using AyBorg.Data.Agent;
using AyBorg.Runtime.Projects;
using AyBorg.Types;

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
        IEnumerable<ProjectMetaRecord> projectMetas = await _projectRepository.GetAllMetasAsync();
        ProjectMetaRecord? projectMeta = projectMetas.FirstOrDefault(p => p.DbId == projectMetaDbId);
        if (projectMeta == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No settings found for project {projectMetaDbId}.", projectMetaDbId);
            return false;
        }

        if (_projectManagementService.ActiveProjectId == projectMeta.Id)
        {
            _engineHost.ActiveProject!.Settings.IsForceResultCommunicationEnabled = projectSettings.IsForceResultCommunicationEnabled;
        }

        _logger.LogInformation(new EventId((int)EventLogType.ProjectState), "Updating project settings for project [{projectName}]: {projectSettings}", projectMeta.Name, projectSettings);

        return true;
    }
}
