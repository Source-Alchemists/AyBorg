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
    public IReadOnlyCollection<IPort> Ports { get; }

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
