using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Plugins.Base;

public sealed class TimeDelay : IStepBody
{
    private readonly NumericPort _milliseconds = new("Milliseconds", PortDirection.Input, 1000, 0);

    /// <inheritdoc />
    public string Name => "Time.Delay";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Time };

    /// <summary>
    /// Initializes a new instance of the <see cref="Delay"/> class.
    /// </summary>
    public TimeDelay()
    {
        Ports = new List<IPort> { _milliseconds };
    }

    /// <inheritdoc />
    public IEnumerable<IPort> Ports { get; }

    /// <inheritdoc />
    public async ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        try
        {
            int targetDelay = Convert.ToInt32(_milliseconds.Value);
            await Task.Delay(targetDelay, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return false;
        }

        return true;
    }
}
