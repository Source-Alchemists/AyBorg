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

internal sealed record PathItem
{
    /// <summary>
    // Gets the identifier of the path item.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the source step.
    /// </summary>
    public IStepProxy Step { get; }

    /// <summary>
    /// Gets the predecessor steps.
    /// </summary>
    public IList<IStepProxy> Predecessors { get; } = new List<IStepProxy>();

    /// <summary>
    /// Gets the successor steps.
    /// </summary>
    public IList<IStepProxy> Successors { get; } = new List<IStepProxy>();

    /// <summary>
    /// Gets a value indicating whether the path item is a start step.
    /// </summary>
    public bool IsStart { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether the path item is a end step.
    /// </summary>
    public bool IsEnd { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether the path item is a merge step.
    /// </summary>
    public bool IsMerge { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether the path item is a fork step.
    /// </summary>
    public bool IsFork { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathItem"/> class.
    /// </summary>
    /// <param name="current">The current step.</param>
    public PathItem(IStepProxy current)
    {
        Id = current.Id;
        Step = current;
    }
}
