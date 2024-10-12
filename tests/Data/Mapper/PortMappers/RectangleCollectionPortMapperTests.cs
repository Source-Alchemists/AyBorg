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

namespace AyBorg.Data.Mapper.Tests;

public class RectangleCollectionPortMapperTests
{
    [Fact]
    public void Test_ToModel()
    {
        // Arrange
        var mapper = new RectangleCollectionPortMapper();
        var port = new RectangleCollectionPort("Test", PortDirection.Input, new List<Rectangle> {
            new() { X = 1, Y = 2, Width = 3, Height = 4},
            new() { X = 5, Y = 6, Width = 7, Height = 8}
         }.ToImmutableList());

        // Act
        PortModel portModel = mapper.ToModel(port);

        // Assert
        Assert.Equal(port.Name, portModel.Name);
        Assert.Equal(port.Direction, portModel.Direction);
        Assert.Equal(JsonSerializer.Serialize(port.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }), portModel.Value);
    }

    [Fact]
    public void Test_ToNative()
    {
        // Arrange
        var mapper = new RectangleCollectionPortMapper();
        var value = new List<RectangleModel> {
            new() { X = 1, Y = 2, Width = 3, Height = 4},
            new() { X = 5, Y = 6, Width = 7, Height = 8}
         }.ToImmutableList();

        // Act
        ImmutableList<Rectangle> nativeValue = mapper.ToNativeValue(value);

        // Assert
        Assert.Equal(value.Count, nativeValue.Count);
        int index = 0;
        foreach(Rectangle n in nativeValue)
        {
            RectangleModel v = value.ElementAt(index);
            Assert.Equal(v.X, n.X);
            Assert.Equal(v.Y, n.Y);
            Assert.Equal(v.Width, n.Width);
            Assert.Equal(v.Height, n.Height);
            index++;
        }
    }

    [Fact]
    public void Test_ToNative_FromJson()
    {
        // Arrange
        var mapper = new RectangleCollectionPortMapper();
        var port = new RectangleCollectionPort("Test", PortDirection.Input, new List<Rectangle> {
            new() { X = 1, Y = 2, Width = 3, Height = 4},
            new() { X = 5, Y = 6, Width = 7, Height = 8}
         }.ToImmutableList());
        string json = JsonSerializer.Serialize(port.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Act
        ImmutableList<Rectangle> nativeValue = mapper.ToNativeValue(json);

        // Assert
        Assert.Equal(new List<Rectangle> {
            new() { X = 1, Y = 2, Width = 3, Height = 4},
            new() { X = 5, Y = 6, Width = 7, Height = 8}
        }.ToImmutableList(), nativeValue);
    }
}
