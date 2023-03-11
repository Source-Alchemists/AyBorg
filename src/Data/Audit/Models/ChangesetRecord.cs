using AyBorg.SDK.Projects;

namespace AyBorg.Data.Audit.Models;

public record ChangesetRecord
{
    public Guid Id { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceUniqueName { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public ProjectState ProjectState { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public int VersionIteration { get; set; }

    public string User { get; set; } = string.Empty;
    public string Approver { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
