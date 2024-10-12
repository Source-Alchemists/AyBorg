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

using ImageTorque;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class ImageScaleTests : IDisposable
{
    private readonly ImageScale _plugin = new();
    private bool _disposedValue;

    [Theory]
    [InlineData(409, 202, 0.5d)]
    [InlineData(819, 404, 1d)]
    public async Task Test_TryRunAsync(int expectedWidth, int expectedHeight, double scaleFactor)
    {
        // Arrange
        using Image testImage = Image.Load("./resources/luna.jpg");
        var imageInputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image"));
        imageInputPort.Value = testImage;
        var imageOutputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Scaled image"));
        var widthPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Width"));
        var heightPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Height"));
        var scalePort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Scale factor"));
        scalePort.Value = scaleFactor;

        // Act
        bool result = await _plugin.TryRunAsync(default);

        // Assert
        Assert.True(result);
        Assert.Equal(testImage.PixelFormat, imageOutputPort.Value.PixelFormat);
        Assert.Equal(expectedWidth, imageOutputPort.Value.Width);
        Assert.Equal(expectedHeight, imageOutputPort.Value.Height);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            _plugin.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
