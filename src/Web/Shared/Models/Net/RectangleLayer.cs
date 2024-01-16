namespace AyBorg.Web.Shared.Models.Net;

public record RectangleLayer
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public int ClassIndex { get; init; }
    public LabelRectangle Shape { get; init; } = null!;
}