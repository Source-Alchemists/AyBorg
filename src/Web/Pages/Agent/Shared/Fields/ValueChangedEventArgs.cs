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

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public class ValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the port.
    /// </summary>
    public PortModel Port { get; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueChangedEventArgs"/> class.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <param name="value">The value.</param>
    public ValueChangedEventArgs(PortModel port, object? value)
    {
        Port = port;
        Value = value;
    }
}
