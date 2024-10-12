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

public sealed class NumericCollectionElementAt : CollectionElementAt, IStepBody
{
    private readonly NumericCollectionPort _inputCollection = new("Collection", PortDirection.Input);
    private readonly NumericPort _outputValue = new("Result", PortDirection.Output, 0);

    public string Name => "Numeric.Collection.ElementAt";

    public NumericCollectionElementAt(ILogger<NumericCollectionElementAt> logger) : base(logger)
    {
        Ports = new List<IPort>
        {
            _inputCollection,
            _inputIndex,
            _outputValue
        };
    }

    protected override void GetAndUpdateElementAt()
    {
        _outputValue.Value = _inputCollection.Value[Convert.ToInt32(_inputIndex.Value)];
    }
}
