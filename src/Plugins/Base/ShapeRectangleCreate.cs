using Autodroid.SDK.Common;
using Autodroid.SDK.Common.Ports;
using Autodroid.SDK.ImageProcessing.Shapes;

namespace Autodroid.Plugins.Base;

public sealed class ShapeRectangleCreate : IStepBody
{
    private readonly NumericPort _xPort = new("X", PortDirection.Input, 0d, 0d);
    private readonly NumericPort _yPort = new("Y", PortDirection.Input, 0d, 0d);
    private readonly NumericPort _widthPort = new("Width", PortDirection.Input, 1d, 1d);
    private readonly NumericPort _heightPort = new("Height", PortDirection.Input, 1d, 1d);
    private readonly RectanglePort _rectanglePort = new("Rectangle", PortDirection.Output, new Rectangle());
    public string DefaultName => "Shape.Rectangle.Create";

    public IEnumerable<IPort> Ports { get; }

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
        _rectanglePort.Value = new Rectangle(System.Convert.ToInt32(_xPort.Value),
                                                System.Convert.ToInt32(_yPort.Value),
                                                System.Convert.ToInt32(_widthPort.Value),
                                                System.Convert.ToInt32(_heightPort.Value));
        return ValueTask.FromResult(true);
    }
}