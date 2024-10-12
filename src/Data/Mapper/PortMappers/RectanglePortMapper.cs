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

using System.Text.Json;
using AyBorg.Data.Agent;
using AyBorg.Types.Models;
using AyBorg.Types.Ports;

namespace AyBorg.Data.Mapper;

public sealed class RectanglePortMapper : IPortMapper<ImageTorque.Rectangle>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public ImageTorque.Rectangle ToNativeValue(object value, Type? type = null)
    {
        if (value is ImageTorque.Rectangle rectangle)
        {
            return rectangle;
        }

        if (value is RectangleModel rectangleModel)
        {
            return new ImageTorque.Rectangle(rectangleModel.X, rectangleModel.Y, rectangleModel.Width, rectangleModel.Height);
        }

        RectangleRecord record = JsonSerializer.Deserialize<RectangleRecord>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        return new ImageTorque.Rectangle(record.X, record.Y, record.Width, record.Height);
    }
    public void Update(IPort port, object value) => ((RectanglePort)port).Value = ToNativeValue(value);
    public PortModel ToModel(IPort port)
    {
        var typedPort = (RectanglePort)port;
        return new PortModel
        {
            Id = port.Id,
            Name = port.Name,
            Direction = port.Direction,
            Brand = port.Brand,
            IsConnected = port.IsConnected,
            IsLinkConvertable = typedPort.IsLinkConvertable,
            Value = typedPort.Value
        };
    }
}
