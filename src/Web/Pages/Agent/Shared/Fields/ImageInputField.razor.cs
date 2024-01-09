using System.Collections.Immutable;
using AyBorg.SDK.Common.Models;
using AyBorg.Web.Shared;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AyBorg.Web.Pages.Agent.Shared.Fields;

public partial class ImageInputField : BaseInputField
{
    [Parameter, EditorRequired] public IReadOnlyCollection<Port> ShapePorts { get; init; } = null!;
    [Parameter] public bool AlternativeMode { get; init; } = false;
    [Inject] public IJSRuntime JSRuntime { get; init; } = null!;

    private readonly string _maskId = $"mask_{Guid.NewGuid()}";
    private readonly List<LabelRectangle> _labelRectangles = new();
    private const int SVG_WIDTH = 250;
    private const int SVG_HEIGHT = 250;
    private string? _imageUrl;
    private ImagePosition _imageInfo;
    private string ImageTooltip => AlternativeMode ? string.Empty : $"Width: {_imageInfo.OrgWidth} Height: {_imageInfo.OrgHeight}";
    private string _imageContainerStyle = string.Empty;
    private string _imageContainerClasses = "flow-node-field mud-full-width mud-full-height px-1 mb-4";
    private int _imageWidth = 0;
    private int _imageHeight = 0;
    private ElementReference _imageContainerRef;
    private BoundingClientRect _boundingClientRect;
    private float _svgScaleFactor = 1f;
    private float _userScaleFactor = 1f;
    private int _svgWidth = SVG_WIDTH;
    private int _svgHeight = SVG_HEIGHT;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        _boundingClientRect = await ElementUtils.GetBoundingClientRectangleAsync(JSRuntime, _imageContainerRef);
        _boundingClientRect = _boundingClientRect with { Width = _boundingClientRect.Width - 10 };
        await CalculateScaleFactorAndUpdateAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (AlternativeMode)
        {
            _imageContainerClasses = "flow-node-field mud-full-width mud-full-height px-1 mb-4 absolute";
            _imageContainerStyle = "top: 70px; left: 0; height: calc(100% - 70px)";
        }
        if (Port == null || Port.Value == null)
        {
            _imageUrl = string.Empty;
        }
        else if (Port.Value is Image image)
        {
            if (AlternativeMode)
            {
                _imageWidth = image.Width;
                _imageHeight = image.Height;
            }
            else
            {
                _imageWidth = SVG_WIDTH;
                _imageHeight = SVG_HEIGHT;
            }
            _imageInfo = new(
                (_imageWidth - image.ScaledWidth) / 2f,
                (_imageHeight - image.ScaledHeight) / 2f,
                image.ScaledWidth,
                image.ScaledHeight,
                image.Width,
                image.Height
            );

            await CalculateScaleFactorAndUpdateAsync();
            SetImageUrl(image);
        }

        _labelRectangles.Clear();
        float scaleFactorX = _imageInfo.FactorX;
        float scaleFactorY = _imageInfo.FactorY;
        foreach (object? value in ShapePorts.Select(sh => sh.Value))
        {
            if (value is Rectangle rectangle)
            {
                AddRectangle(scaleFactorX, scaleFactorY, rectangle);
            }
            else if (value is ImmutableList<Rectangle> rectangeCollection)
            {
                foreach (Rectangle rect in rectangeCollection)
                {
                    AddRectangle(scaleFactorX, scaleFactorY, rect);
                }
            }
        }

        await base.OnParametersSetAsync();
    }

    private void AddRectangle(float scaleFactorX, float scaleFactorY, Rectangle rectangle)
    {
        float rectWidth = rectangle.Width * scaleFactorX;
        float rectHeight = rectangle.Height * scaleFactorY;
        _labelRectangles.Add(new LabelRectangle(
                                    _imageInfo.X + (rectangle.X * scaleFactorX) - (rectWidth / 2),
                                    _imageInfo.Y + (rectangle.Y * scaleFactorY) - (rectHeight / 2),
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

    private async ValueTask CalculateScaleFactorAndUpdateAsync()
    {
        (float s, int w, int h) = CalcSvgScaleFactor();
        if (s != _svgScaleFactor)
        {
            _svgScaleFactor = s;
            _svgWidth = w;
            _svgHeight = h;
            await InvokeAsync(StateHasChanged);
        }
    }

    private (float, int, int) CalcSvgScaleFactor()
    {
        float scaleFactorW = 1f;
        float scaleFactorH = 1f;
        if (AlternativeMode)
        {
            scaleFactorW = (float)_boundingClientRect.Width / _imageWidth;
            scaleFactorH = (float)_boundingClientRect.Height / _imageHeight;
        }

        float scaleFactor = MathF.Min(scaleFactorW, scaleFactorH);
        scaleFactor *= _userScaleFactor;
        return (scaleFactor, (int)(_imageWidth * scaleFactor), (int)(_imageHeight * scaleFactor));
    }

    private async void OnFitToScreen()
    {
        _userScaleFactor = 1f;
        await CalculateScaleFactorAndUpdateAsync();
    }

    private async void OnZoomIn()
    {
        _userScaleFactor += 0.1f;
        await CalculateScaleFactorAndUpdateAsync();
    }

    private async void OnZoomOut()
    {
        _userScaleFactor -= 0.1f;
        _userScaleFactor = MathF.Max(1f, _userScaleFactor);
        await CalculateScaleFactorAndUpdateAsync();
    }
}
