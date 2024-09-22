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

using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class ImageMirrorTests : IDisposable
{
    private readonly ImageMirror _plugin = new();
    private bool _disposedValue;

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Test_TryRunAsync(bool mirrorVertical, bool mirrorHorizontal)
    {
        // Arrange
        using Image testImage = Image.Load("./resources/luna.jpg");
        var imageInputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image"));
        imageInputPort.Value = testImage;
        var imageOutputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Mirrored image"));
        var mirrorVerticalPort = (BooleanPort)_plugin.Ports.First(p => p.Name.Equals("Vertical"));
        var mirrorHorizonzalPort = (BooleanPort)_plugin.Ports.First(p => p.Name.Equals("Horizontal"));
        mirrorVerticalPort.Value = mirrorVertical;
        mirrorHorizonzalPort.Value = mirrorHorizontal;

        // Act
        bool result = await _plugin.TryRunAsync(default);

        // Assert
        Assert.True(result);
        Assert.Equal(testImage.PixelFormat, imageOutputPort.Value.PixelFormat);
        Assert.Equal(testImage.Width, imageOutputPort.Value.Width);
        Assert.Equal(testImage.Height, imageOutputPort.Value.Height);
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
