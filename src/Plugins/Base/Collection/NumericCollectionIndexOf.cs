using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Collection;

public sealed class NumericCollectionIndexOf : IStepBody
{
    private readonly ILogger<NumericCollectionIndexOf> _logger;
    private readonly NumericCollectionPort _inputCollection = new("Collection", PortDirection.Input);
    private readonly NumericPort _inputSearchValue = new("Value", PortDirection.Input, 0);
    private readonly NumericPort _outputIndex = new("Index", PortDirection.Output, 0);

    public string DefaultName => "Numeric.Collection.IndexOf";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Collection };

    public IEnumerable<IPort> Ports { get; }

    public NumericCollectionIndexOf(ILogger<NumericCollectionIndexOf> logger)
    {
        _logger = logger;
        Ports = new List<IPort>
        {
            _inputCollection,
            _inputSearchValue,
            _outputIndex
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _outputIndex.Value = _inputCollection.Value.IndexOf(_inputSearchValue.Value);
        if (_outputIndex.Value == -1)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Value {value} not found in collection", _inputSearchValue.Value);
            return ValueTask.FromResult(false);
        }
        return ValueTask.FromResult(true);
    }
}
