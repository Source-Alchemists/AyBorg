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
