using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Collection;

public sealed class NumericCollectionElementAt : CollectionElementAt, IStepBody
{
    private readonly NumericCollectionPort _inputCollection = new("Collection", PortDirection.Input);
    private readonly NumericPort _outputValue = new("Result", PortDirection.Output, 0);

    public string Name => "Numeric.Collection.ElementAt";

    public NumericCollectionElementAt(ILogger<NumericCollectionElementAt> logger) : base(logger)
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
