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
using AyBorg.Runtime.Projects;

namespace AyBorg.Web.Shared.Models.Agent;

public sealed record ProjectSaveInfo
{
    [Required]
    public ProjectState State { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string? VersionName { get; set; }

    [StringLength(200)]
    public string? Comment { get; set; }

    public string? UserName { get; set; }
}
