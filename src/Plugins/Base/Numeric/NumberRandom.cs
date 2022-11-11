using Autodroid.SDK.Common;
using Autodroid.SDK.Common.Ports;

namespace Autodroid.Plugins.Base.Numeric;

public sealed class NumberRandom : IStepBody
{
    private readonly NumericPort _seedPort = new("Seed", PortDirection.Input, -1, -1, int.MaxValue);
    private readonly NumericPort _minPort = new("Min", PortDirection.Input, 0);
    private readonly NumericPort _maxPort = new("Max", PortDirection.Input, 1);
    private readonly NumericPort _resultPort = new("Result", PortDirection.Output, 0);

    private Random _random = null!;
    private int _seed = -1;

    public string DefaultName => "Number.Random";

    public IEnumerable<IPort> Ports { get; }

    public NumberRandom()
    {
        Ports = new IPort[] { _seedPort, _minPort, _maxPort, _resultPort };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        if (_random == null || _seed != Convert.ToInt32(_seedPort.Value))
        {
            _seed = Convert.ToInt32(_seedPort.Value == -1 ? (int)DateTime.Now.Ticks : _seedPort.Value);
            _random = new Random(_seed);
        }
        _resultPort.Value = _random.Next(Convert.ToInt32(_minPort.Value), Convert.ToInt32(_maxPort.Value) + 1);
        return ValueTask.FromResult(true);
    }
}