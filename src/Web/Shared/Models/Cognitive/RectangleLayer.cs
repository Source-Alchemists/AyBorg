namespace AyBorg.Web.Shared.Models.Cognitive;

public record RectangleLayer
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public int ClassIndex { get; init; }
    public LabelRectangle Shape { get; init; } = null!;
}
