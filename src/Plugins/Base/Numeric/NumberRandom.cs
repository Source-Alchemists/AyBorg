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

public sealed class NumberRandom : IStepBody
{
    private readonly NumericPort _seedPort = new("Seed", PortDirection.Input, -1, -1, int.MaxValue);
    private readonly NumericPort _minPort = new("Min", PortDirection.Input, 0);
    private readonly NumericPort _maxPort = new("Max", PortDirection.Input, 1);
    private readonly NumericPort _resultPort = new("Result", PortDirection.Output, 0);

    private Random _random = null!;
    private int _seed = -1;

    public string Name => "Number.Random";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Math };

    public IReadOnlyCollection<IPort> Ports { get; }

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
