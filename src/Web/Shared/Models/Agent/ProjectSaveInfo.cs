using System.ComponentModel.DataAnnotations;
using AyBorg.SDK.Projects;

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
