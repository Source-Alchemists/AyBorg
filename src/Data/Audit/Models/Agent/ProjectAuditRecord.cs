namespace AyBorg.Data.Audit.Models.Agent;

public record ProjectAuditRecord : ChangesetRecord
{
    public ProjectSettingsAuditRecord Settings { get; set; } = new();
    public List<StepAuditRecord> Steps { get; set; } = new();
    public List<LinkAuditRecord> Links { get; set; } = new();
}
