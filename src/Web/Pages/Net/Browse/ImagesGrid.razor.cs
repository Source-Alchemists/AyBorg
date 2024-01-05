using System.Collections.Immutable;
using AyBorg.SDK.Common;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Net.Browse;

public partial class ImagesGrid : ComponentBase
{
    [Parameter, EditorRequired] public string ProjectId { get; init; } = string.Empty;
    [Parameter, EditorRequired] public IEnumerable<string> ImageNames { get; init; } = null!;

    private const int MAX_IMAGES_PER_PAGE = 40;
    private IEnumerable<IEnumerable<string>> _imageNameBatches = new List<List<string>>();
    private ImmutableList<string> _lastImageNames = ImmutableList<string>.Empty;

    private ImmutableList<string> _selectedImageNameBatch = ImmutableList<string>.Empty;
    private int _selectedPage = 1;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        _selectedPage = 1;

        if (ImageNames.Any() && _lastImageNames.SequenceEqual(ImageNames))
        {
            return;
        }

        _lastImageNames = _lastImageNames.Clear();
        _lastImageNames = _lastImageNames.AddRange(ImageNames);

        _imageNameBatches = ImageNames.Batch(MAX_IMAGES_PER_PAGE);
        if (_imageNameBatches.Any())
        {
            _selectedImageNameBatch = _selectedImageNameBatch.Clear();
            _selectedImageNameBatch = _selectedImageNameBatch.AddRange(_imageNameBatches.First());
        }
    }

    private void OnSelectedPageChanged(int value)
    {
        _selectedPage = value;
        IEnumerable<string>? batch = _imageNameBatches.ElementAtOrDefault(value - 1);
        if (batch == null)
        {
            return;
        }

        _selectedImageNameBatch = _selectedImageNameBatch.Clear();
        _selectedImageNameBatch = _selectedImageNameBatch.AddRange(batch);
    }
}