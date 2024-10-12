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

using AyBorg.Diagrams.Core.Models;
using AyBorg.Types.Models;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

public class FlowNode : NodeModel
{
    /// <summary>
    /// Gets the step.
    /// </summary>
    public StepModel Step { get; private set; }

    /// <summary>
    /// Called when a step is updated.
    /// </summary>
    public Action StepChanged { get; set; } = null!;

    /// <summary>
    /// Called when a step is deleted.
    /// </summary>
    public Action OnDelete { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowNode"/> class.
    /// </summary>
    /// <param name="flowService">The flow service.</param>
    /// <param name="step">The step.</param>
    /// <param name="locked">Whether the node is locked.</param>
    public FlowNode(StepModel step, bool locked = false) : base(step.Id.ToString(), new Diagrams.Core.Geometry.Point(step.X, step.Y))
    {
        Title = step.Name;
        Step = step;
        Locked = locked;

        if (step.Ports == null) return;
        foreach (Types.Models.PortModel port in step.Ports)
        {
            _ = AddPort(new FlowPort(this, port) { Locked = locked });
        }
    }

    /// <summary>
    /// Updates the step.
    /// </summary>
    /// <param name="newStep">The new step.</param>
    public void Update(StepModel newStep)
    {
        Step.ExecutionTimeMs = newStep.ExecutionTimeMs;

        foreach (FlowPort targetFlowPort in Ports.Cast<FlowPort>())
        {
            Types.Models.PortModel sourcePort = newStep.Ports!.FirstOrDefault(p => p != null && p.Id.Equals(targetFlowPort.Port.Id))!;
            if (sourcePort == null)
            {
                continue;
            }
            targetFlowPort.Update(targetFlowPort.Port with { Value = sourcePort.Value });
        }

        StepChanged?.Invoke();
    }

    /// <summary>
    /// Deletes the step.
    /// </summary>
    public void Delete() => OnDelete?.Invoke();
}
