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

using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class ImageCropTests : IDisposable
{
    private static readonly NullLogger<ImageCrop> s_logger = new();
    private readonly ImageCrop _plugin = new(s_logger);
    private bool _disposedValue;

    [Theory]
    [InlineData(true, 10, 10)]
    [InlineData(false, 0, 10)]
    [InlineData(false, 10, 0)]
    public async Task Test_TryRunAsync(bool expectedResult, int width, int height)
    {
        // Arrange
        using Image testImage = Image.Load("./resources/luna.jpg");
        var imageInputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image"));
        imageInputPort.Value = testImage;
        var imageOutputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Cropped image"));
        var regionPort = (RectanglePort)_plugin.Ports.First(p => p.Name.Equals("Region"));
        regionPort.Value = new Rectangle(0, 0, width, height);

        // Act
        bool result = await _plugin.TryRunAsync(default);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.Equal(width, imageOutputPort.Value.Width);
            Assert.Equal(height, imageOutputPort.Value.Height);
        }
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
