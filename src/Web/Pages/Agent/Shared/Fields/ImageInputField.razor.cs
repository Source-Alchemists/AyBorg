using System.Collections.ObjectModel;
using AyBorg.SDK.Common.Models;
using AyBorg.Web.Pages.Agent.Editor.Nodes;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class ImageInputField : BaseInputField
{
    [Parameter, EditorRequired] public IReadOnlyCollection<FlowPort> ShapePorts { get; init; } = null!;

    private readonly string _maskId = $"mask_{Guid.NewGuid()}";
    private readonly List<LabelRectangle> _labelRectangles = new();
    private const int SVG_WIDTH = 250;
    private const int SVG_HEIGHT = 250;
    private string? _imageUrl;
    private ImagePosition _imagePosition;
    private string ImageTooltip => $"Width: {_imagePosition.OrgWidth} Height: {_imagePosition.OrgHeight}";

    protected override void OnParametersSet()
    {
        if (Port == null || Port.Value == null)
        {
            _imageUrl = string.Empty;
        }
        else if (Port.Value is Image image)
        {
            _imagePosition = new(
                (SVG_WIDTH - image.ScaledWidth) / 2f,
                (SVG_HEIGHT - image.ScaledHeight) / 2f,
                image.ScaledWidth,
                image.ScaledHeight,
                image.Width,
                image.Height
            );
            SetImageUrl(image);
        }

        _labelRectangles.Clear();
        float scaleFactorX = _imagePosition.FactorX;
        float scaleFactorY = _imagePosition.FactorY;
        foreach(Port shapePort in ShapePorts.Select(p => p.Port))
        {
            if(shapePort.Value is Rectangle rectangle)
            {
                AddRectangle(scaleFactorX, scaleFactorY, rectangle);
            }
            else if(shapePort.Value is ReadOnlyCollection<Rectangle> rectangeCollection)
            {
                foreach (Rectangle rect in rectangeCollection)
                {
                    AddRectangle(scaleFactorX, scaleFactorY, rect);
                }
            }
        }

        base.OnParametersSet();
    }

    private void AddRectangle(float scaleFactorX, float scaleFactorY, Rectangle rectangle)
    {
        float rectWidth = rectangle.Width * scaleFactorX;
        float rectHeight = rectangle.Height * scaleFactorY;
        _labelRectangles.Add(new LabelRectangle(
                                    _imagePosition.X + (rectangle.X * scaleFactorX) - (rectWidth / 2),
                                    _imagePosition.Y + (rectangle.Y * scaleFactorY) - (rectHeight / 2),
                                    rectWidth,
                                    rectHeight
        ));
    }

    private void SetImageUrl(Image image)
    {
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

    private record struct ImagePosition(float X, float Y, float Width, float Height, float OrgWidth, float OrgHeight)
    {
        public float FactorX => Width / OrgWidth;
        public float FactorY => Height / OrgHeight;
    }

    private sealed record LabelRectangle(float X, float Y, float Width, float Height)
    {
        public string Color { get; } = "#af4ae2ff";
        public string FillColor { get; } = "transparent";
    }
}
