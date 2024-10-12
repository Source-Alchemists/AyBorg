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

namespace AyBorg.Agent.Runtime;

public interface IEngine : IDisposable
{
    /// <summary>
    /// Called when the iteration is started.
    /// </summary>
    event EventHandler<IterationStartedEventArgs> IterationStarted;

    /// <summary>
    /// Called when the iteration is finished.
    /// </summary>
    event EventHandler<IterationFinishedEventArgs> IterationFinished;

    /// <summary>
    /// Called when the engine state is changed.
    /// </summary>
    event EventHandler<EngineState>? StateChanged;

    /// <summary>
    /// Gets the meta information.
    /// </summary>
    EngineMeta Meta { get; }

    /// <summary>
    /// Gets the execution type.
    /// </summary>
    EngineExecutionType ExecutionType { get; }

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <returns></returns>
    ValueTask<bool> TryStartAsync();

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns></returns>
    ValueTask<bool> TryStopAsync();

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns></returns>
    ValueTask<bool> TryAbortAsync();
}
