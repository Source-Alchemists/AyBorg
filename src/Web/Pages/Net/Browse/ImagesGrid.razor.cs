using AyBorg.SDK.Common;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Net.Browse;

public partial class ImagesGrid : ComponentBase
{
    [Parameter, EditorRequired] public IEnumerable<string> ImageNames { get; init; } = null!;

    private const int MAX_IMAGES_PER_PAGE = 50;
    private IEnumerable<IEnumerable<string>> _imageNameBatches = new List<List<string>>();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if(firstRender)
        {
            _imageNameBatches = ImageNames.Batch(MAX_IMAGES_PER_PAGE);
            await InvokeAsync(StateHasChanged);
        }
    }
}