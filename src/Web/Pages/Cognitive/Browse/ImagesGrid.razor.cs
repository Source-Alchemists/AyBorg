/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Immutable;
using AyBorg.Runtime;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Cognitive.Browse;

public partial class ImagesGrid : ComponentBase
{
    [Parameter, EditorRequired] public string ProjectId { get; init; } = string.Empty;
    [Parameter, EditorRequired] public IEnumerable<string> ImageNames { get; init; } = null!;
    [Parameter, EditorRequired] public List<string> SelectedImageNames { get; init; } = null!;
    [Parameter, EditorRequired] public IEnumerable<string> DatasetImageNames { get; init; } = null!;
    [Parameter] public EventCallback OnThumbnailSelectionChanged { get; set; }
    [Parameter] public EventCallback<string> OnThumbnailAnnotateClicked { get; set; }

    private const int MAX_IMAGES_PER_PAGE = 20;
    private IEnumerable<IEnumerable<string>> _imageNameBatches = new List<List<string>>();
    private ImmutableList<string> _lastImageNames = ImmutableList<string>.Empty;

    private ImmutableList<string> _selectedImageNameBatch = ImmutableList<string>.Empty;
    private int _selectedPage = 1;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (ImageNames.Any() && _lastImageNames.SequenceEqual(ImageNames))
        {
            return;
        }

        _selectedPage = 1;

        _lastImageNames = _lastImageNames.Clear();
        _lastImageNames = _lastImageNames.AddRange(ImageNames);

        _imageNameBatches = ImageNames.Batch(MAX_IMAGES_PER_PAGE);
        if (_imageNameBatches.Any())
        {
            _selectedImageNameBatch = _selectedImageNameBatch.Clear();
            _selectedImageNameBatch = _selectedImageNameBatch.AddRange(_imageNameBatches.First());
        }
    }

    private void SelectedPageChanged(int value)
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

    private async Task ThumbnailSelectChanged(ImageThumbnail.SelectChangedArgs args)
    {
        bool exists = SelectedImageNames.Exists(n => n.Equals(args.ImageName, StringComparison.InvariantCultureIgnoreCase));
        if (args.Value)
        {
            if (!exists)
            {
                SelectedImageNames.Add(args.ImageName);
                await OnThumbnailSelectionChanged.InvokeAsync();
            }
        }
        else
        {
            if (exists)
            {
                SelectedImageNames.Remove(args.ImageName);
                await OnThumbnailSelectionChanged.InvokeAsync();
            }
        }
    }

    private async Task ThumbnailAnnotateClicked(string value)
    {
        await OnThumbnailAnnotateClicked.InvokeAsync(value);
    }

    private bool IsSelectedImageName(string imageName)
    {
        return SelectedImageNames.Exists(x => x.Equals(imageName, StringComparison.InvariantCultureIgnoreCase));
    }

    private bool IsImageUsedInDataset(string imageName)
    {
        return DatasetImageNames.Any(x => x.Equals(imageName, StringComparison.InvariantCultureIgnoreCase));
    }
}
