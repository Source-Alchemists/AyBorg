/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using AyBorg.Types;
using AyBorg.Types.Ports;

namespace AyBorg.Plugins.Base.Numeric;

public sealed class NumberMathFunction : IStepBody
{
    private readonly EnumPort _functionPort = new("Function", PortDirection.Input, MathFunctions.Add);
    private readonly NumericPort _valueAPort = new("Value A", PortDirection.Input, 0);
    private readonly NumericPort _valueBPort = new("Value B", PortDirection.Input, 0);
    private readonly NumericPort _resultPort = new("Result", PortDirection.Output, 0);

    public string Name => "Number.Math.Function";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Math };

    public IReadOnlyCollection<IPort> Ports { get; }

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
