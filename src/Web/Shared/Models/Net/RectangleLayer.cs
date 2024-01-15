namespace AyBorg.Web.Shared.Models.Net;

public record RectangleLayer
{
    public int ClassIndex { get; init; }
    public LabelRectangle Shape { get; init; } = null!;
}