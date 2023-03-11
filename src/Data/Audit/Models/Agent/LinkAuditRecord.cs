namespace AyBorg.Data.Audit.Models.Agent;

public record LinkAuditRecord
{
    public Guid Id { get; set; }
    public Guid SourceId { get; set; }
    public Guid TargetId { get; set; }
}
