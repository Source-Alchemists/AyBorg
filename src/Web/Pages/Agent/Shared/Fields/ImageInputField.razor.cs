using System.Collections.Immutable;
using AyBorg.SDK.Common.Models;
using AyBorg.Web.Shared;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class ImageInputField : BaseInputField
{
    [Parameter, EditorRequired] public IReadOnlyCollection<Port> ShapePorts { get; init; } = null!;
    [Parameter] public bool OnlyThumbnail { get; init; } = false;

    private readonly List<LabelRectangle> _labelRectangles = new();
    private string? _imageUrl;
    private Rectangle _imagePosition;
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

            _imagePosition = new Rectangle {
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
            if (value is Rectangle rectangle)
            {
                AddRectangle(rectangle);
            }
            else if (value is ImmutableList<Rectangle> rectangeCollection)
            {
                foreach (Rectangle rect in rectangeCollection)
                {
                    AddRectangle(rect);
                }
            }
        }

        await base.OnParametersSetAsync();
    }

    private void AddRectangle(Rectangle rectangle)
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
        if(string.IsNullOrEmpty(image.Base64))
        {
            _imageUrl = string.Empty;
            return;
        }

        switch (image.EncoderType)
        {
            case ImageTorque.Processing.EncoderType.Jpeg:
                _imageUrl = $"data:image/jpeg;base64,{image.Base64}";
                break;
            case ImageTorque.Processing.EncoderType.Png:
                _imageUrl = $"data:image/png;base64,{image.Base64}";
                break;
            case ImageTorque.Processing.EncoderType.Bmp:
                _imageUrl = $"data:image/bmp;base64,{image.Base64}";
                break;
        }
    }
}
