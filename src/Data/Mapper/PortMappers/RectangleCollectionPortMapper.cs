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
using ImageTorque;

namespace AyBorg.Data.Mapper;

public class RectangleCollectionPortMapper : IPortMapper<ImmutableList<Rectangle>>
{
    public PortModel ToModel(IPort port)
    {
        var typedPort = (RectangleCollectionPort)port;
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

    public ImmutableList<Rectangle> ToNativeValue(object value, Type? type = null)
    {
        List<Rectangle> record;
        if (value is ImmutableList<RectangleModel> collection)
        {
            record = new List<Rectangle>();
            foreach (RectangleModel rec in collection)
            {
                record.Add(new Rectangle { X = rec.X, Y = rec.Y, Width = rec.Width, Height = rec.Height });
            }
        }
        else
        {
            record = JsonSerializer.Deserialize<List<Rectangle>>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        return record.ToImmutableList();
    }

    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);

    public void Update(IPort port, object value) => ((RectangleCollectionPort)port).Value = ToNativeValue(value);
}
