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
