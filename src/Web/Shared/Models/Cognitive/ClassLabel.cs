namespace AyBorg.Web.Shared.Models.Cognitive;

public record ClassLabel
{
    public string Name { get; init; } = string.Empty;
    public string ColorCode { get; init; } = "#C100E7";
    public int Index { get; init; }
}
