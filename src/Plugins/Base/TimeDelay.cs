using Autodroid.SDK.Common;
using Autodroid.SDK.Common.Ports;

namespace Autodroid.Plugins.Base;

public sealed class TimeDelay : IStepBody
{
    private readonly NumericPort _milliseconds = new("Milliseconds", PortDirection.Input, 1000, 0);

    /// <inheritdoc />
    public string DefaultName => "Time.Delay";

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
            var targetDelay = System.Convert.ToInt32(_milliseconds.Value);
            await Task.Delay(targetDelay, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return false;
        }

        return true;
    }
}