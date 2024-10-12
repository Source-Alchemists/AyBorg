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

namespace AyBorg.Web.Services.Agent;

public interface IRuntimeService
{
    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The status.</returns>
    ValueTask<EngineMeta> GetStatusAsync(string serviceUniqueName);

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns>The status</returns>
    ValueTask<EngineMeta> StartRunAsync(string serviceUniqueName, EngineExecutionType executionType);

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The status</returns>
    ValueTask<EngineMeta> StopRunAsync(string serviceUniqueName);

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The status</returns>
    ValueTask<EngineMeta> AbortRunAsync(string serviceUniqueName);
}
