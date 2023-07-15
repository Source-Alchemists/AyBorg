using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Collection;

public abstract class CollectionElementAt
{
    private readonly ILogger<CollectionElementAt> _logger;
    protected readonly NumericPort _inputIndex = new("Index", PortDirection.Input, 0);

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Collection };

    public IReadOnlyCollection<IPort> Ports { get; protected init; } = null!;

    protected CollectionElementAt(ILogger<CollectionElementAt> logger)
    {
        _logger = logger;
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        try
        {
            GetAndUpdateElementAt();
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

    protected abstract void GetAndUpdateElementAt();
}
