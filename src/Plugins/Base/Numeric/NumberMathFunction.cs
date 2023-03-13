using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Plugins.Base.Numeric;

public sealed class NumberMathFunction : IStepBody
{
    private readonly EnumPort _functionPort = new("Function", PortDirection.Input, MathFunctions.Add);
    private readonly NumericPort _valueAPort = new("Value A", PortDirection.Input, 0);
    private readonly NumericPort _valueBPort = new("Value B", PortDirection.Input, 0);
    private readonly NumericPort _resultPort = new("Result", PortDirection.Output, 0);

    public string DefaultName => "Number.Math.Function";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Math };

    public IEnumerable<IPort> Ports { get; }

    public NumberMathFunction()
    {
        Ports = new IPort[] { _functionPort, _valueAPort, _valueBPort, _resultPort };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        switch (_functionPort.Value)
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

        return ValueTask.FromResult(true);
    }
}
