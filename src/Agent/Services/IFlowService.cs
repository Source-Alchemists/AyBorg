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
using AyBorg.Types.Ports;

namespace AyBorg.Agent.Services;

public interface IFlowService
{
    /// <summary>
    /// Get steps.
    /// </summary>
    /// <returns></returns>
    IEnumerable<IStepProxy> GetSteps();

    /// <summary>
    /// Gets the links.
    /// </summary>
    IEnumerable<PortLink> GetLinks();

    /// <summary>
    /// Gets the port.
    /// </summary>
    /// <param name="portId">The port identifier.</param>
    /// <returns></returns>
    IPort GetPort(Guid portId);

    /// <summary>
    /// Adds the step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>The new created step proxy.</returns>
    ValueTask<IStepProxy> AddStepAsync(Guid stepId, int x, int y);

    /// <summary>
    /// Removes the step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <returns></returns>
    ValueTask<bool> TryRemoveStepAsync(Guid stepId);

    /// <summary>
    /// Moves the step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    ValueTask<bool> TryMoveStepAsync(Guid stepId, int x, int y);

    /// <summary>
    /// Link ports together.
    /// </summary>
    /// <param name="sourcePortId">The source port identifier.</param>
    /// <param name="targetPortId">The target port identifier.</param>
    /// <returns></returns>
    ValueTask<PortLink> LinkPortsAsync(Guid sourcePortId, Guid targetPortId);

    /// <summary>
    /// Unlink ports.
    /// </summary>
    /// <param name="linkId">The link identifier.</param>
    /// <returns></returns>
    ValueTask<bool> TryUnlinkPortsAsync(Guid linkId);

    /// <summary>
    /// Updates the port value.
    /// </summary>
    /// <param name="portId">The port identifier.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    ValueTask<bool> TryUpdatePortValueAsync(Guid portId, object value);
}
