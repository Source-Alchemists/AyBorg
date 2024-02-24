using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Shared.ImageSelection;

public partial class ImageThumbnail : ComponentBase
{
    [Parameter] public ImageSource Source { get; init; } = null!;
    [Parameter] public int Width { get; init; } = 100;
    [Parameter] public int Height { get; init; } = 100;

    private string _containerStyle = "width: 100px; height: 100px;";

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        await base.OnAfterRenderAsync(firstRender);

         if (firstRender)
         {
            _containerStyle = $"width: {Width}px; height: {Height}px;";
            await InvokeAsync(StateHasChanged);
         }
    }
}
