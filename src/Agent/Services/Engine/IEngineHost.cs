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

using AyBorg.Agent.Runtime;
using AyBorg.Runtime;
using AyBorg.Runtime.Projects;

namespace AyBorg.Agent.Services;

public interface IEngineHost : IDisposable
{
    /// <summary>
    /// Occurs when [iteration started].
    /// </summary>
    event EventHandler<IterationStartedEventArgs> IterationStarted;

    /// <summary>
    /// Occurs when [iteration finished].
    /// </summary>
    event EventHandler<IterationFinishedEventArgs> IterationFinished;

    /// <summary>
    /// Gets the active project.
    /// </summary>
    Project? ActiveProject { get; }

    /// <summary>
    /// Tries to activate the specified project.
    /// </summary>
    /// <param name="project">The project.</param>
    ValueTask<bool> TryActivateProjectAsync(Project project);

    /// <summary>
    /// Tries to deactivate the project.
    /// </summary>
    /// <returns></returns>
    ValueTask<bool> TryDeactivateProjectAsync();

    /// <summary>
    /// Gets the engine status.
    /// </summary>
    /// <returns></returns>
    EngineMeta GetEngineStatus();

    /// <summary>
    /// Start the engine.
    /// </summary>
    /// <param name="executionType">The execution type.</param>
    /// <returns>Engine meta informations.</returns>
    ValueTask<EngineMeta> StartRunAsync(EngineExecutionType executionType);

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>Engine meta informations.</returns>
    ValueTask<EngineMeta> StopRunAsync();

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>Engine meta informations.</returns>
    ValueTask<EngineMeta> AbortRunAsync();
}
