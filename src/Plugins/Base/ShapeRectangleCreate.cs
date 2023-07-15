using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Plugins.Base;

public sealed class ShapeRectangleCreate : IStepBody
{
    private readonly NumericPort _xPort = new("X", PortDirection.Input, 0d, 0d);
    private readonly NumericPort _yPort = new("Y", PortDirection.Input, 0d, 0d);
    private readonly NumericPort _widthPort = new("Width", PortDirection.Input, 1d, 1d);
    private readonly NumericPort _heightPort = new("Height", PortDirection.Input, 1d, 1d);
    private readonly RectanglePort _rectanglePort = new("Rectangle", PortDirection.Output, new Rectangle());

    public string Name => "Shape.Rectangle.Create";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing, DefaultStepCategories.ImageShapes };

    public IReadOnlyCollection<IPort> Ports { get; }

    public ShapeRectangleCreate()
    {
        Ports = new List<IPort>
        {
            _xPort,
            _yPort,
            _widthPort,
            _heightPort,
            _rectanglePort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _rectanglePort.Value = new Rectangle(Convert.ToInt32(_xPort.Value),
                                                Convert.ToInt32(_yPort.Value),
                                                Convert.ToInt32(_widthPort.Value),
                                                Convert.ToInt32(_heightPort.Value));
        return ValueTask.FromResult(true);
    }
}
