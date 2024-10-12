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
using AyBorg.Types.Ports;

namespace AyBorg.Agent.Services;

public interface IRuntimeConverterService
{
    /// <summary>
    /// Converts the specified project.
    /// </summary>
    /// <param name="projectRecord">The project.</param>
    /// <returns></returns>
    ValueTask<Project> ConvertAsync(ProjectRecord projectRecord);

    /// <summary>
    /// Updates the port value.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    ValueTask<bool> TryUpdatePortValueAsync(IPort port, object value);

    /// <summary>
    /// Updates the port values.
    /// </summary>
    /// <param name="ports">The ports.</param>
    /// <param name="portRecords">The port records.</param>
    ValueTask UpdateValuesAsync(IEnumerable<IPort> ports, IEnumerable<PortRecord> portRecords);
}
