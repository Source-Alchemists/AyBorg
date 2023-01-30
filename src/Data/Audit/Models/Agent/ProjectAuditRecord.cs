using AyBorg.SDK.Projects;

namespace AyBorg.Data.Audit.Models.Agent;

public record ProjectAuditRecord
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ProjectState State { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public int VersionIteration { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string SavedBy { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public ProjectSettingsAuditRecord Settings { get; set; } = new();
    public List<StepAuditRecord> Steps { get; set; } = new();
    public List<LinkAuditRecord> Links { get; set; } = new();
}
