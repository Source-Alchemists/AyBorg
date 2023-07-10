using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Collection;

public sealed class StringCollectionElementAt : CollectionElementAt, IStepBody
{
    private readonly StringCollectionPort _inputCollection = new("Collection", PortDirection.Input);
    private readonly StringPort _outputValue = new("Result", PortDirection.Output, string.Empty);

    public string Name => "String.Collection.ElementAt";

    public StringCollectionElementAt(ILogger<StringCollectionElementAt> logger) : base(logger)
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
