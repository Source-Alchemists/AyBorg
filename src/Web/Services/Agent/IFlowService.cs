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

using AyBorg.Types.Models;
using AyBorg.Web.Shared;

namespace AyBorg.Web.Services.Agent;

public interface IFlowService
{
    event EventHandler<PortValueChangedEventArgs> PortValueChanged;

    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <returns>The steps.</returns>
    ValueTask<IEnumerable<StepModel>> GetStepsAsync();

    /// <summary>
    /// Gets the links.
    /// </summary>
    /// <returns>The links.</returns>
    ValueTask<IEnumerable<LinkModel>> GetLinksAsync();

    /// <summary>
    /// Adds the step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns>Added step.</returns>
    ValueTask<StepModel> AddStepAsync(StepModel step);

    /// <summary>
    /// Removes the step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns></returns>
    ValueTask<bool> TryRemoveStepAsync(StepModel step);

    /// <summary>
    /// Moves the step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    ValueTask<bool> TryMoveStepAsync(Guid stepId, int x, int y);

    /// <summary>
    /// Add link between ports.
    /// </summary>
    /// <param name="sourcePort">The source port.</param>
    /// <param name="targetPort">The target port.</param>
    /// <returns></returns>
    ValueTask<Guid?> AddLinkAsync(PortModel sourcePort, PortModel targetPort);

    /// <summary>
    /// Removes the link.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <returns></returns>
    ValueTask<bool> TryRemoveLinkAsync(LinkModel link);

    /// <summary>
    /// Gets the port for the given iteration.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="portId">The port identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="asThumbnail">Whether to get the thumbnail.</param>
    /// <returns></returns>
    ValueTask<PortModel> GetPortAsync(string agentUniqueName, Guid portId, Guid? iterationId = null, bool asThumbnail = true);

    /// <summary>
    /// Gets the step.
    /// </summary>
    /// <param name="agentUniqueName">The agent unique name.</param>
    /// <param name="originalStep">The original step.</param>
    /// <param name="iterationId">The iteration id.</param>
    /// <param name="updatePorts">Whether to update the ports.</param>
    /// <param name="skipOutputPorts">Whether to skip output ports.</param>
    /// <param name="asThumbnail">Whether to get the thumbnail.</param>
    /// <returns>The step.</returns>
    ValueTask<StepModel> GetStepAsync(string agentUniqueName, StepModel originalStep, Guid? iterationId = null, bool updatePorts = true, bool skipOutputPorts = true, bool asThumbnail = true);

    /// <summary>
    /// Gets the link.
    /// </summary>
    /// <param name="linkId">The link identifier.</param>
    /// <returns>The link.</returns>
    ValueTask<LinkModel> GetLinkAsync(Guid linkId);

    /// <summary>
    /// Try to set the port value.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    ValueTask<bool> TrySetPortValueAsync(PortModel port);
}
