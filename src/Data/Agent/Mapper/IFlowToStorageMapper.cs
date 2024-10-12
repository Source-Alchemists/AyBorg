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

using AyBorg.Runtime;
using AyBorg.Runtime.Projects;

namespace AyBorg.Data.Agent;

public interface IFlowToStorageMapper
{
    /// <summary>
    /// Maps the specified step proxy.
    /// </summary>
    /// <param name="stepProxy">The step proxy.</param>
    /// <returns></returns>
    StepRecord Map(IStepProxy stepProxy);

    /// <summary>
    /// Maps the specified project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    ProjectRecord Map(Project project);
}
