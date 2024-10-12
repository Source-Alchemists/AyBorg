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

using AyBorg.Runtime.Projects;

namespace AyBorg.Web.Shared.Models;

public sealed record AuditChangeset
{
    public Guid Token { get; init; }
    public string ServiceType { get; init; } = string.Empty;
    public string ServiceUniqueName { get; init; } = string.Empty;
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public ProjectState ProjectState { get; init; }
    public string VersionName { get; init; } = string.Empty;
    public int VersionIteration { get; init; }
    public string User { get; init; } = string.Empty;
    public string Approver { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
