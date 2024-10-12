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

namespace AyBorg.Agent.Services;

public interface IProjectManagementService
{
    /// <summary>
    /// Gets the active project identifier.
    /// </summary>
    Guid ActiveProjectId { get; }

    /// <summary>
    /// Changes the activation state.
    /// </summary>
    /// <param name="projectMetaDbId">The project database identifier.</param>
    /// <param name="isActive">if set to <c>true</c> [is active].</param>
    /// <returns></returns>
    ValueTask<ProjectManagementResult> TryChangeActivationStateAsync(Guid projectMetaDbId, bool isActive);

    /// <summary>
    /// Creates the project.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    ValueTask<ProjectRecord> CreateAsync(string name);

    /// <summary>
    /// Deletes the project.
    /// </summary>
    /// <param name="projectMetaId">The project meta id.</param>
    /// <returns></returns>
    ValueTask<ProjectManagementResult> TryDeleteAsync(Guid projectMetaId);

    /// <summary>
    /// Gets all project metas.
    /// </summary>
    ValueTask<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync();

    /// <summary>
    /// Load active project.
    /// </summary>
    ValueTask<ProjectManagementResult> TryLoadActiveAsync();

    /// <summary>
    /// Save active project.
    /// </summary>
    /// <param name="userName">Name of the user saving the project.</param>
    /// <returns></returns>
    ValueTask<ProjectManagementResult> TrySaveActiveAsync(string userName);

    /// <summary>
    /// Save the project as new version.
    /// </summary>
    ValueTask<ProjectManagementResult> TrySaveAsync(Guid projectMetaDbId, ProjectState projectState, string newVersionName, string approver, string comment);
}
