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
using AyBorg.Types.Models;
using AyBorg.Web.Shared;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class ImageInputField : BaseInputField
{
    [Parameter, EditorRequired] public IReadOnlyCollection<PortModel> ShapePorts { get; init; } = null!;
    [Parameter] public bool OnlyThumbnail { get; init; } = false;

    private readonly List<LabelRectangle> _labelRectangles = new();
    private string? _imageUrl;
    private RectangleModel _imagePosition;
    private int _imageWidth = 0;
    private int _imageHeight = 0;

    protected override async Task OnParametersSetAsync()
    {
        if (Port == null || Port.Value == null)
        {
            _imageUrl = string.Empty;
        }
        else if (Port.Value is Image image)
        {
            _imageWidth = image.Width;
            _imageHeight = image.Height;

            _imagePosition = new RectangleModel
            {
                X = 0,
                Y = 0,
                Width = _imageWidth,
                Height = _imageHeight
            };

            SetImageUrl(image);
        }

        _labelRectangles.Clear();
        foreach (object? value in ShapePorts.Select(sh => sh.Value))
        {
            if (value is RectangleModel rectangle)
            {
                AddRectangle(rectangle);
            }
            else if (value is ImmutableList<RectangleModel> rectangeCollection)
            {
                foreach (RectangleModel rect in rectangeCollection)
                {
                    AddRectangle(rect);
                }
            }
        }

        await base.OnParametersSetAsync();
    }

    private void AddRectangle(RectangleModel rectangle)
    {
        float rectWidth = rectangle.Width;
        float rectHeight = rectangle.Height;
        _labelRectangles.Add(new LabelRectangle(
                                    _imagePosition.X + rectangle.X - (rectWidth / 2),
                                    _imagePosition.Y + rectangle.Y - (rectHeight / 2),
                                    rectWidth,
                                    rectHeight
        ));
    }

    private void SetImageUrl(Image image)
    {
        if (string.IsNullOrEmpty(image.Base64))
        {
            _imageUrl = string.Empty;
            return;
        }

        _imageUrl = image.EncoderType switch
        {
            "jpeg" => $"data:image/jpeg;base64,{image.Base64}",
            "png" => $"data:image/png;base64,{image.Base64}",
            "bmp" => $"data:image/bmp;base64,{image.Base64}",
            _ => throw new NotSupportedException($"The encoder type '{image.EncoderType}' is not supported."),
        };
    }
}
