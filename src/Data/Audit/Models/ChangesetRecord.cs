using AyBorg.SDK.Projects;
using AyBorg.SDK.System;

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
    public AuditEntryType Type { get; set; } = AuditEntryType.Unknown;
}
