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

public sealed class StringCollectionIndexOf : IStepBody
{
    private readonly ILogger<StringCollectionIndexOf> _logger;
    private readonly StringCollectionPort _inputCollection = new("Collection", PortDirection.Input);
    private readonly StringPort _inputSearchValue = new("Value", PortDirection.Input, string.Empty);
    private readonly NumericPort _outputIndex = new("Index", PortDirection.Output, 0);

    public string Name => "String.Collection.IndexOf";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Collection };

    public IReadOnlyCollection<IPort> Ports { get; }

    public StringCollectionIndexOf(ILogger<StringCollectionIndexOf> logger)
    {
        _logger = logger;
        Ports = new List<IPort>
        {
            _inputCollection,
            _inputSearchValue,
            _outputIndex
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _outputIndex.Value = _inputCollection.Value.IndexOf(_inputSearchValue.Value);
        if (_outputIndex.Value == -1)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Value {value} not found in collection", _inputSearchValue.Value);
            return ValueTask.FromResult(false);
        }
        return ValueTask.FromResult(true);
    }
}
