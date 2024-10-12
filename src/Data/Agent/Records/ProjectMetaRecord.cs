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

namespace AyBorg.Data.Agent;

public record ProjectMetaRecord
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [Key]
    public Guid DbId { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <remarks>Used to identifie the project in combination with the version.</remarks>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [Editable(true)]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the updated date.
    /// </summary>
    /// <value>
    /// The updated date.
    /// </value>
    public DateTime UpdatedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is active.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
    /// </value>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    public ProjectState State { get; set; } = ProjectState.Draft;

    /// <summary>
    /// Gets or sets the unique name of the service. As hint wich service is responsible for this project.
    /// </summary>
    public string ServiceUniqueName { get; set; } = string.Empty;

    /// <summary>
    // Gets or sets the version of this project.
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string VersionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the comment.
    /// </summary>
    [Editable(true)]
    [StringLength(200)]
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the internal version.
    /// </summary>
    public long VersionIteration { get; set; } = 0;

    /// <summary>
    /// Gets or sets the project record identifier.
    /// </summary>
    /// <remarks>Used for navigation.</remarks>
    public Guid ProjectRecordId { get; set; }

    /// <summary>
    /// Gets or sets the approvers.
    /// </summary>
    public string ApprovedBy { get; set; } = string.Empty;
}
