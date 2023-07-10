using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Collection;

public sealed class RectangleCollectionElementAt : CollectionElementAt, IStepBody
{
    private readonly RectangleCollectionPort _inputCollection = new("Collection", PortDirection.Input);
    private readonly RectanglePort _outputValue = new("Result", PortDirection.Output, new Rectangle());

    public string Name => "Rectangle.Collection.ElementAt";

    public RectangleCollectionElementAt(ILogger<RectangleCollectionElementAt> logger) : base(logger)
    {
        Ports = new List<IPort>
        {
            _inputCollection,
            _inputIndex,
            _outputValue
        };
    }

    protected override void GetAndUpdateElementAt()
    {
        _outputValue.Value = _inputCollection.Value.ElementAt(Convert.ToInt32(_inputIndex.Value));
    }
}
