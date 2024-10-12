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

using AyBorg.Communication;
using AyBorg.Runtime.Projects;

namespace AyBorg.Agent.Services;

public sealed record CommunicationStateProvider : ICommunicationStateProvider
{
    /// <summary>
    /// Gets a value indicating whether the result communication is enabled.
    /// </summary>
    public bool IsResultCommunicationEnabled { get; private set; }

    /// <summary>
    /// Updates the communication state.
    /// </summary>
    /// <param name="project">The project.</param>
    public void Update(Project project)
    {
        IsResultCommunicationEnabled = project.Meta.State == ProjectState.Ready;

        if (project.Settings.IsForceResultCommunicationEnabled)
        {
            IsResultCommunicationEnabled = !IsResultCommunicationEnabled;
        }
    }
}
