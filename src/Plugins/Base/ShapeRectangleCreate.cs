using Atomy.SDK;
using Atomy.SDK.ImageProcessing.Shapes;
using Atomy.SDK.Ports;

namespace Atomy.Plugins.Base;

public sealed class ShapeRectangleCreate : IStepBody
{
    private readonly NumericPort _xPort = new NumericPort("X", PortDirection.Input, 0d, 0d);
    private readonly NumericPort _yPort = new NumericPort("Y", PortDirection.Input, 0d, 0d);
    private readonly NumericPort _widthPort = new NumericPort("Width", PortDirection.Input, 1d, 1d);
    private readonly NumericPort _heightPort = new NumericPort("Height", PortDirection.Input, 1d, 1d);
    private readonly RectanglePort _rectanglePort = new RectanglePort("Rectangle", PortDirection.Output, new Rectangle());
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

    public Task<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _rectanglePort.Value = new Rectangle(System.Convert.ToInt32(_xPort.Value), 
                                                System.Convert.ToInt32(_yPort.Value), 
                                                System.Convert.ToInt32(_widthPort.Value), 
                                                System.Convert.ToInt32(_heightPort.Value));
        return Task.FromResult(true);
    }
}