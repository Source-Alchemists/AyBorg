using AyBorg.SDK.Common.Models;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Shared.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
    [Parameter] public bool DrawCrosshairVisible { get; init; } = true;
    [Parameter] public DrawMode DrawMode { get; set; } = DrawMode.None;
    [Parameter] public EventCallback<DrawMode> DrawModeChanged { get; init; }
    [Parameter] public string StatusBarText { get; init; } = string.Empty;
    [Parameter] public EventCallback<string> StatusBarTextChanged { get; init; }
    [Parameter] public EventCallback<Rectangle> OnShapeDrawed { get; init; }
    [Inject] public IJSRuntime JSRuntime { get; init; } = null!;
    private readonly string _maskId = $"mask_{Guid.NewGuid()}";
    private ElementReference _containerRef;
    private BoundingClientRect _boundingClientRect;
    private string _containerStyle = "height: calc(100% - 90px)";
    private string _cursorClass = "default-cursor";
    private string _centerClass = "force-center";
    private Rectangle _imagePosition = new();
    private int _svgWidth = 0;
    private int _svgHeight = 0;
    private float _svgScaleFactor = 1f;
    private float _userScaleFactor = 1f;
    private string _imageTooltip => $"Width: {_imagePosition.Width} Height: {_imagePosition.Height}\nX: {_drawCrossHairX.Position1.X} Y: {_drawCrossHairY.Position1.Y}";
    private Line _drawCrossHairX = new();
    private Line _drawCrossHairY = new();
    private bool _isCrossHairVisible = false;
    private bool _isCursorCaptured = false;
    private DrawObject _drawObject = new();

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

        _containerStyle = ToolbarVisible ? "height: calc(100% - 90px)" : string.Empty;
        _cursorClass = DrawCrosshairVisible ? "draw-cursor" : "default-cursor";

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

            if (_svgWidth > _boundingClientRect.Width || _svgHeight > _boundingClientRect.Height)
            {
                _centerClass = string.Empty;
            }
            else
            {
                _centerClass = "force-center";
            }

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

    private void SvgMouseDown(MouseEventArgs args)
    {
        if (DrawMode == DrawMode.None)
        {
            return;
        }

        int x = (int)(args.OffsetX / _svgScaleFactor);
        int y = (int)(args.OffsetY / _svgScaleFactor);
        _drawObject = new DrawObject
        {
            IsDrawing = true,
            StartPosition = new Point
            {
                X = x,
                Y = y
            },
            EndPosition = new Point
            {
                X = x,
                Y = y
            }
        };

        _drawObject = _drawObject with
        {
            Rect = CalculateDrawingRectangle(_drawObject)
        };
    }

    private async Task SvgMouseUp(MouseEventArgs args)
    {
        if (DrawMode == DrawMode.None)
        {
            return;
        }

        _drawObject = _drawObject with
        {
            IsDrawing = false,
            EndPosition = new Point
            {
                X = (int)(args.OffsetX / _svgScaleFactor),
                Y = (int)(args.OffsetY / _svgScaleFactor)
            }
        };

        _drawObject = _drawObject with
        {
            Rect = CalculateDrawingRectangle(_drawObject)
        };

        await DrawModeChanged.InvokeAsync(DrawMode.None);
        await StatusBarTextChanged.InvokeAsync(string.Empty);
        await OnShapeDrawed.InvokeAsync(_drawObject.Rect);
    }

    private void SvgMouseEnter(MouseEventArgs args)
    {
        if (DrawCrosshairVisible)
        {
            _isCrossHairVisible = true;
        }

        _isCursorCaptured = true;
    }

    private void SvgMouseLeave(MouseEventArgs args)
    {
        _isCrossHairVisible = false;
        _isCursorCaptured = false;
    }

    private void SvgMouseMove(MouseEventArgs args)
    {
        
        SvgMove(args.OffsetX, args.OffsetY);
    }

    private void SvgMove(double xOffset, double yOffset)
    {
        int x = (int)(xOffset / _svgScaleFactor);
        int y = (int)(yOffset / _svgScaleFactor);

        _drawCrossHairY = new()
        {
            Position1 = new() { X = 0, Y = y },
            Position2 = new() { X = ImageWidth, Y = y }
        };

        _drawCrossHairX = new()
        {
            Position1 = new() { X = x, Y = 0 },
            Position2 = new() { X = x, Y = ImageHeight }
        };

        _drawObject = _drawObject with
        {
            EndPosition = new Point
            {
                X = x,
                Y = y
            }
        };

        _drawObject = _drawObject with
        {
            Rect = CalculateDrawingRectangle(_drawObject)
        };
    }

    private static Rectangle CalculateDrawingRectangle(DrawObject drawObject)
    {
        int minX = Math.Min(drawObject.StartPosition.X, drawObject.EndPosition.X);
        int minY = Math.Min(drawObject.StartPosition.Y, drawObject.EndPosition.Y);
        int maxX = Math.Max(drawObject.StartPosition.X, drawObject.EndPosition.X);
        int maxY = Math.Max(drawObject.StartPosition.Y, drawObject.EndPosition.Y);

        return new Rectangle
        {
            X = minX,
            Y = minY,
            Width = maxX - minX,
            Height = maxY - minY
        };
    }

    private readonly record struct DrawObject
    {
        public bool IsDrawing { get; init; }
        public Point StartPosition { get; init; }
        public Point EndPosition { get; init; }
        public Rectangle Rect { get; init; }
    }
}