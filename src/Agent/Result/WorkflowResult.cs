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

using System.Collections.Concurrent;
using AyBorg.Types.Result;

namespace AyBorg.Agent.Result;

public record WorkflowResult
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid IterationId { get; init; }
    public DateTime StartTime { get; set; }
    public DateTime StopTime { get; set; }
    public int ElapsedMs { get; set; }
    public bool Success { get; set; }
    public BlockingCollection<PortResult> PortResults { get; } = new BlockingCollection<PortResult>();
}
