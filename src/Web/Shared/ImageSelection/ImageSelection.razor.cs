using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Shared.ImageSelection;

public partial class ImageSelection : ComponentBase
{
    [Parameter] public IEnumerable<ImageSource> Sources { get; init; } = Array.Empty<ImageSource>();
    [Parameter] public int ItemSize { get; init; } = 100;
    [Parameter] public ImageSource SelectedItem { get; init; } = null!;
    [Parameter] public EventCallback<ImageSource> OnSelectionChanged { get; init; }

    private ImageSource _selectedImageSource = null!;

    protected override async Task OnParametersSetAsync() {
      await base.OnParametersSetAsync();
      _selectedImageSource = SelectedItem;
    }

    private async Task OnThumbnailSelected(ImageSource value)
    {
        _selectedImageSource = value;
        await OnSelectionChanged.InvokeAsync(value);
    }
}
