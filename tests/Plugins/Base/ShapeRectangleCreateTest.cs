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

using AyBorg.Types.Ports;

namespace AyBorg.Plugins.Base.Tests;

public class ShapeRectangleCreateTests
{
    private readonly ShapeRectangleCreate _plugin = new();

    [Fact]
    public async Task Test_TryRunAsync()
    {
        // Arrange
        var xPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("X"));
        var yPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Y"));
        var widthPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Width"));
        var heightPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Height"));
        var rectanglePort = (RectanglePort)_plugin.Ports.First(p => p.Name.Equals("Rectangle"));
        xPort.Value = 1;
        yPort.Value = 2;
        widthPort.Value = 3;
        heightPort.Value = 4;

        // Act
        bool result = await _plugin.TryRunAsync(default);

        // Assert
        Assert.True(result);
        Assert.Equal(xPort.Value, rectanglePort.Value.X);
        Assert.Equal(yPort.Value, rectanglePort.Value.Y);
        Assert.Equal(widthPort.Value, rectanglePort.Value.Width);
        Assert.Equal(heightPort.Value, rectanglePort.Value.Height);
    }
}
