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

namespace AyBorg.Web.Shared.Models;

public record AgentServiceEntry
{
    public string Name { get; set; }
    public string EditorLink { get; }
    public string ProjectsLink { get; }
    public string DevicesLink { get; }
    public string ActiveProjectName { get; init; } = string.Empty;
    public EngineMeta Status { get; init; } = new EngineMeta();

    public AgentServiceEntry(ServiceInfoEntry serviceInfoEntry)
    {
        Name = serviceInfoEntry.Name.Replace("AyBorg.", string.Empty);
        EditorLink = $"agents/editor/{serviceInfoEntry.Id}";
        ProjectsLink = $"agents/projects/{serviceInfoEntry.Id}";
        DevicesLink = $"agents/devices/{serviceInfoEntry.Id}";
    }
}
