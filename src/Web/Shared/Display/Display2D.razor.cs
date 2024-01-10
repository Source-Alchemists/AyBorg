using AyBorg.SDK.Common.Models;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AyBorg.Web.Shared.Display;

public partial class Display2D : ComponentBase
{
    [Parameter, EditorRequired] public string Base64Image { get; init; } = string.Empty;
    [Parameter, EditorRequired] public int ImageWidth { get; init; }
    [Parameter, EditorRequired] public int ImageHeight { get; init; }
    [Parameter] public int ContainerWidth { get; init; } = -1;
    [Parameter] public int ContainerHeight { get; init; } = -1;
    [Parameter] public IEnumerable<LabelRectangle> Shapes { get; init; } = Array.Empty<LabelRectangle>();
    [Parameter] public bool ToolbarVisible { get; init; } = true;
    [Inject] public IJSRuntime JSRuntime { get; init; } = null!;
    private readonly string _maskId = $"mask_{Guid.NewGuid()}";
    private ElementReference _containerRef;
    private BoundingClientRect _boundingClientRect;
    private string _containerStyle = "height: calc(100% - 60px)";
    private Rectangle _imagePosition = new();
    private int _svgWidth;
    private int _svgHeight;
    private float _svgScaleFactor = 1f;
    private float _userScaleFactor = 1f;

    private string _imageTooltip => $"Width: {_imagePosition.Width} Height: {_imagePosition.Height}";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        _boundingClientRect = await ElementUtils.GetBoundingClientRectangleAsync(JSRuntime, _containerRef);

        // Workaround for cases where the parent container gets scaled.
        if (ContainerWidth > 0 && ContainerHeight > 0)
        {
            _boundingClientRect = _boundingClientRect with { Width = ContainerWidth, Height = ContainerHeight };
        }
        await CalculateScaleFactorAndUpdateAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        _containerStyle = ToolbarVisible ? "height: calc(100% - 60px)" : string.Empty;

        _imagePosition = new Rectangle
        {
            X = 0,
            Y = 0,
            Width = ImageWidth,
            Height = ImageHeight
        };

        await CalculateScaleFactorAndUpdateAsync();
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
        float scaleFactorW = (float)_boundingClientRect.Width / _imagePosition.Width;
        float scaleFactorH = (float)_boundingClientRect.Height / _imagePosition.Height;
        float scaleFactor = MathF.Min(scaleFactorW, scaleFactorH);
        scaleFactor *= _userScaleFactor;
        return (scaleFactor, (int)(_imagePosition.Width * scaleFactor), (int)(_imagePosition.Height * scaleFactor));
    }

    private async Task FitToScreen()
    {
        _userScaleFactor = 1f;
        await CalculateScaleFactorAndUpdateAsync();
    }

    private async Task ZoomIn()
    {
        _userScaleFactor += 0.1f;
        await CalculateScaleFactorAndUpdateAsync();
    }

    private async Task ZoomOut()
    {
        _userScaleFactor -= 0.1f;
        _userScaleFactor = MathF.Max(1f, _userScaleFactor);
        await CalculateScaleFactorAndUpdateAsync();
    }
}