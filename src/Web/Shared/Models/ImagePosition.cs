namespace AyBorg.Web.Shared;

public record struct ImagePosition(float X, float Y, float Width, float Height, float OrgWidth, float OrgHeight)
{
    public readonly float FactorX => Width / OrgWidth;
    public readonly float FactorY => Height / OrgHeight;
}