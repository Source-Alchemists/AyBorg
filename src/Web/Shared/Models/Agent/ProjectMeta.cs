using System.ComponentModel.DataAnnotations;
using AyBorg.SDK.Projects;

namespace AyBorg.Web.Shared.Models.Agent;

public sealed record ProjectMeta
{

    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    [Required]
    public string DbId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [Editable(true)]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version name.
    /// </summary>
    [Editable(true)]
    [StringLength(100, MinimumLength = 1)]
    public string VersionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the comment.
    /// </summary>
    [Editable(true)]
    [StringLength(200)]
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the updated date.
    /// </summary>
    public DateTime ChangeDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    public ProjectState State { get; set; } = ProjectState.Draft;

    /// <summary>
    /// Gets or sets the approvers.
    /// </summary>
    public string? ApprovedBy { get; set; }

    public ProjectMeta() { }

    public ProjectMeta(Ayborg.Gateway.V1.ProjectMeta projectMeta)
    {
        DbId = projectMeta.DbId;
        Id = projectMeta.Id;
        Name = projectMeta.Name;
        VersionName = projectMeta.VersionName;
        Comment = projectMeta.Comment;
        CreationDate = projectMeta.CreationDate.ToDateTime();
        ChangeDate = projectMeta.ChangeDate.ToDateTime();
        IsActive = projectMeta.IsActive;
        State = (ProjectState)projectMeta.State;
        ApprovedBy = projectMeta.ApprovedBy;
    }
}
