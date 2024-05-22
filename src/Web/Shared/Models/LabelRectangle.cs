namespace AyBorg.Web.Shared;

public sealed record LabelRectangle(float X, float Y, float Width, float Height)
{
    public string Color { get; init; } = "#af4ae2ff";
    public string FillColor { get; } = "transparent";
}