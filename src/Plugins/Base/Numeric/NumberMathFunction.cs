using Atomy.SDK.Common;
using Atomy.SDK.Common.Ports;

namespace Atomy.Plugins.Base.Numeric;

public sealed class NumberMathFunction : IStepBody
{
    private readonly EnumPort _functionPort = new EnumPort("Function", PortDirection.Input, MathFunctions.Add);
    private readonly NumericPort _valueAPort = new NumericPort("Value A", PortDirection.Input, 0);
    private readonly NumericPort _valueBPort = new NumericPort("Value B", PortDirection.Input, 0);
    private readonly NumericPort _resultPort = new NumericPort("Result", PortDirection.Output, 0);
    public string DefaultName => "Number.Math.Function";
    public IEnumerable<IPort> Ports { get; }

    public NumberMathFunction()
    {
        Ports = new IPort[] { _functionPort, _valueAPort, _valueBPort, _resultPort };
    }

    public Task<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        switch(_functionPort.Value)
        {
            case MathFunctions.Add:
                _resultPort.Value = _valueAPort.Value + _valueBPort.Value;
                break;
            case MathFunctions.Subtract:
                _resultPort.Value = _valueAPort.Value - _valueBPort.Value;
                break;
            case MathFunctions.Multiply:
                _resultPort.Value = _valueAPort.Value * _valueBPort.Value;
                break;
            case MathFunctions.Divide:
                _resultPort.Value = _valueAPort.Value / _valueBPort.Value;
                break;
        }

        return Task.FromResult(true);
    }
}