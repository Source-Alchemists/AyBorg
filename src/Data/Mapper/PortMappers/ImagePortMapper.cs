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

using AyBorg.Types.Models;
using AyBorg.Types.Ports;
using ImageTorque;

namespace AyBorg.Data.Mapper;

public sealed class ImagePortMapper : IPortMapper<Image>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public Image ToNativeValue(object value, Type? type = null) => null!;
    public void Update(IPort port, object value) => ((ImagePort)port).Value = ToNativeValue(value);
    public PortModel ToModel(IPort port)
    {
        var typedPort = (ImagePort)port;

        CacheImage cacheImage;
        if (typedPort.Value != null)
        {
            cacheImage = new CacheImage
            {
                Meta = new ImageMeta
                {
                    Width = typedPort.Value.Width,
                    Height = typedPort.Value.Height,
                    PixelFormat = typedPort.Value.PixelFormat,
                },
                OriginalImage = new Image(typedPort.Value)
            };
        }
        else
        {
            cacheImage = new CacheImage();
        }

        return new PortModel
        {
            Id = port.Id,
            Name = port.Name,
            Direction = port.Direction,
            Brand = port.Brand,
            IsConnected = port.IsConnected,
            IsLinkConvertable = typedPort.IsLinkConvertable,
            Value = cacheImage
        };
    }
}
