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

public class StringCollectionPortMapper : IPortMapper<ImmutableList<string>>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public ImmutableList<string> ToNativeValue(object value, Type? type = null)
    {
        List<string> record;
        if (value is ImmutableList<string> collection)
        {
            record = collection.ToList();
        }
        else
        {
            record = JsonSerializer.Deserialize<List<string>>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        // Check for null strings as we only allow empty strings, not null.
        if (record.Any(r => r == null))
        {
            var newCollection = new List<string>();
            foreach (string s in record)
            {
                if (s == null)
                {
                    newCollection.Add(string.Empty);
                }
                else
                {
                    newCollection.Add(s);
                }
            }
            record = newCollection;
        }

        return record.ToImmutableList();
    }

    public void Update(IPort port, object value) => ((StringCollectionPort)port).Value = ToNativeValue(value);
    public PortModel ToModel(IPort port)
    {
        var typedPort = (StringCollectionPort)port;
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
