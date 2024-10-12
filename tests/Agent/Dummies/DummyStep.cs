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

namespace AyBorg.Agent.Tests.Dummies;

/// <summary>
/// We need a "real" step instead of a mock to test the valid behaviour.
/// </summary>
/// <seealso cref="AyBorg.SDK.IStepBody" />
public class DummyStep : IStepBody
{
    public DummyStep()
    {
        Ports = new List<IPort>
        {
            new StringPort("String input", PortDirection.Input, "Test"),
            new NumericPort("Numeric input", PortDirection.Input, 123),
            new NumericPort("Output", PortDirection.Output, 123, 100, 200),
        };
    }

    public string Name => "Dummy";
    public IReadOnlyCollection<string> Categories { get; } = new List<string>();
    public IReadOnlyCollection<IPort> Ports { get; }
    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
