using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Net.Models;

public partial class ModelInspect : ComponentBase
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Parameter] public string ModelId { get; init; } = string.Empty;
    private bool _isLoading = true;
    private string _projectName = string.Empty;
    private string _modelName = string.Empty;
    private HashSet<ImageSource> _imageSources = new();

    private async Task ImageAdded(ImageSource image)
    {
        if(_imageSources.Any(i => i.Hash.Equals(image.Hash)))
        {
            return;
        }

        _imageSources.Add(image);
        await InvokeAsync(StateHasChanged);
    }
}
