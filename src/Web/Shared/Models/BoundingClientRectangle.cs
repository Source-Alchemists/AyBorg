namespace AyBorg.Web.Shared.Models;

internal record struct BoundingClientRectangle(
    double X,
    double Y,
    double Width,
    double Height,
    double Top,
    double Right,
    double Bottom,
    double Left
);
