namespace AyBorg.Data.Audit.Models;

public record ChangeRecord
{
    public Guid ChangesetAId { get; init; }
    public Guid ChangesetBId { get; init; }
    public string Label { get; init; } = string.Empty;
    public string SubLabel { get; init; } = string.Empty;
    public string ValueA { get; init; } = string.Empty;
    public string ValueB { get; init; } = string.Empty;
}
