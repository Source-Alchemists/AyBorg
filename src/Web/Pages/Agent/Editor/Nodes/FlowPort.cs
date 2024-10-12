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
using AyBorg.Types.Ports;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

public class FlowPort : PortModel
{
    /// <summary>
    /// Gets the port name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the port direction.
    /// </summary>
    public PortDirection Direction { get; }

    /// <summary>
    /// Gets the port type.
    /// </summary>
    public PortBrand Brand { get; }

    /// <summary>
    /// Gets the port dto.
    /// </summary>
    public Types.Models.PortModel Port { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowPort"/> class.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="port">The port.</param>
    public FlowPort(FlowNode node, Types.Models.PortModel port)
            : base(port.Id.ToString(), node, port.Direction == PortDirection.Input ? PortAlignment.Left : PortAlignment.Right)
    {
        Port = port;
        Name = port.Name;
        Direction = port.Direction;
        Brand = port.Brand;
    }

    /// <summary>
    /// Updates the port.
    /// </summary>
    public void Update(Types.Models.PortModel newPort)
    {
        if (newPort.Id == Guid.Empty) return;
        Port = newPort;
    }
}
