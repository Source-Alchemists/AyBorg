using System.Collections.ObjectModel;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Collection;

public sealed class StringCollectionElementAt : IStepBody
{
    private readonly ILogger<StringCollectionElementAt> _logger;
    private readonly StringCollectionPort _inputCollection = new("Collection", PortDirection.Input, new ReadOnlyCollection<string>(Array.Empty<string>()));
    private readonly NumericPort _inputIndex = new("Index", PortDirection.Input, 0);
    private readonly StringPort _outputValue = new("Result", PortDirection.Output, string.Empty);

    public string DefaultName => "String.Collection.ElementAt";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Collection };

    public IEnumerable<IPort> Ports { get; }

    public StringCollectionElementAt(ILogger<StringCollectionElementAt> logger)
    {
        _logger = logger;
        Ports = new List<IPort>
        {
            _inputCollection,
            _inputIndex,
            _outputValue
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        try
        {
            _outputValue.Value = _inputCollection.Value.ElementAt(Convert.ToInt32(_inputIndex.Value));
            return ValueTask.FromResult(true);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Plugin), ex, "Index {index} is out of range", _inputIndex.Value);
            return ValueTask.FromResult(false);
        }
        catch (OverflowException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Plugin), ex, "Index {index} is too large", _inputIndex.Value);
            return ValueTask.FromResult(false);
        }
    }

}
