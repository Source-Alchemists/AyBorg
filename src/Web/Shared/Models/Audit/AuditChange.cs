namespace AyBorg.Web.Shared.Models;

public sealed record AuditChange
{
    public Guid ChangesetTokenA { get; init; }
    public Guid ChangesetTokenB { get; init; }
    public string Label { get; init; } = string.Empty;
    public string SubLabel { get; init; } = string.Empty;
    public string ValueA { get; init; } = string.Empty;
    public string ValueB { get; init; } = string.Empty;
}
