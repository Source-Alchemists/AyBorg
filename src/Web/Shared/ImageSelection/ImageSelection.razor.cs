using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Shared.ImageSelection;

public partial class ImageSelection : ComponentBase
{
    [Parameter] public IEnumerable<ImageSource> Sources { get; init; } = Array.Empty<ImageSource>();
    [Parameter] public int ItemSize { get; init; } = 100;
}
