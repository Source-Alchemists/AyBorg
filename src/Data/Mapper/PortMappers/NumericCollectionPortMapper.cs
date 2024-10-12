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

using System.Collections.Immutable;
using System.Text.Json;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;

namespace AyBorg.Data.Mapper;

public sealed class NumericCollectionPortMapper : IPortMapper<ImmutableList<double>>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public ImmutableList<double> ToNativeValue(object value, Type? type = null)
    {
        if (value is ImmutableList<double> collection)
        {
            return collection;
        }
        else
        {
            return JsonSerializer.Deserialize<List<double>>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.ToImmutableList();
        }
    }
    public void Update(IPort port, object value) => ((NumericCollectionPort)port).Value = ToNativeValue(value);
    public PortModel ToModel(IPort port)
    {
        var typedPort = (NumericCollectionPort)port;
        return new PortModel
        {
            Id = port.Id,
            Name = port.Name,
            Direction = port.Direction,
            Brand = port.Brand,
            IsConnected = port.IsConnected,
            IsLinkConvertable = typedPort.IsLinkConvertable,
            Value = JsonSerializer.Serialize(typedPort.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        };
    }
}
