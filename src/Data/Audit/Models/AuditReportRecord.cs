namespace AyBorg.Data.Audit.Models;

public sealed record AuditReportRecord
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public List<ChangesetRecord> Changesets { get; init; } = new();
}
