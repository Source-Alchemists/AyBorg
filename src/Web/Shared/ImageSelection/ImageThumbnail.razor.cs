using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Shared.ImageSelection;

public partial class ImageThumbnail : ComponentBase
{
    [Parameter] public ImageSource Source { get; init; } = null!;
    [Parameter] public int Width { get; init; } = 100;
    [Parameter] public int Height { get; init; } = 100;
    [Parameter] public bool Select { get; init; } = false;
    [Parameter] public EventCallback<ImageSource> OnSelectChanged { get; init; }

    private string _selectClass => Select ? "image-thumbnail-container-selected" : string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        await base.OnAfterRenderAsync(firstRender);

         if (firstRender)
         {
            await InvokeAsync(StateHasChanged);
         }
    }

    private async Task ImageClicked()
    {
        await OnSelectChanged.InvokeAsync(Source);
    }
}
