using System.Collections.ObjectModel;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Collection;

public sealed class RectangleCollectionElementAt : IStepBody
{
    private readonly ILogger<RectangleCollectionElementAt> _logger;
    private readonly RectangleCollectionPort _inputCollection = new("Collection", PortDirection.Input, new ReadOnlyCollection<Rectangle>(Array.Empty<Rectangle>()));
    private readonly NumericPort _inputIndex = new("Index", PortDirection.Input, 0);
    private readonly RectanglePort _outputValue = new("Result", PortDirection.Output, new Rectangle());

    public string DefaultName => "Rectangle.Collection.ElementAt";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Collection };

    public IEnumerable<IPort> Ports { get; }

    public RectangleCollectionElementAt(ILogger<RectangleCollectionElementAt> logger)
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
