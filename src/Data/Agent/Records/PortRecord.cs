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

using System.ComponentModel.DataAnnotations;
using AyBorg.Types.Ports;

namespace AyBorg.Data.Agent;

#nullable disable

public abstract record PortRecord
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    [Key]
    public Guid DbId { get; init; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the direction.
    /// </summary>
    public PortDirection Direction { get; init; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the brand.
    /// </summary>
    public PortBrand Brand { get; init; }
}
