namespace AyBorg.Web.Shared.Models;

public sealed record AuditReport
{
    public Guid Id { get; init; } = Guid.Empty;
    public DateTime Timestamp { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public List<AuditChangeset> Changesets { get; init; } = new();
    public int ChangesetCount => Changesets.Count;
}
