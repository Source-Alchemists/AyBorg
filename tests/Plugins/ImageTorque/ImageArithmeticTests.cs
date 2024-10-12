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
using ImageTorque.Pixels;

using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class ImageArithemticTests : IDisposable
{
    private static readonly NullLogger<ImageArithemtic> s_logger = new();
    private readonly ImageArithemtic _plugin = new(s_logger);
    private bool _disposedValue;

    [Theory]
    [InlineData("950a62510266b34a8641ac3bde1201b7eddc2a53e21d0f243e143bfc48ba1295", ImageMathMode.Add)]
    [InlineData("c81ca5eda5947c7826ad046fdbdc2a25a846b835a6c34c237cc8b3afbe9ec6cc", ImageMathMode.Subtract)]
    [InlineData("e0f8ddd1b7060ffd612faab8710eb00153ec80ec3df37c3d40bbebbe58d08a93", ImageMathMode.Multiply)]
    public async Task Test_TryRunAsync(string expectedResult, ImageMathMode mode)
    {
        // Arrange
        using Image testImageA = Image.Load("./resources/luna.jpg");
        using Image testImageB = Image.Load("./resources/luna.jpg");
        var imagePortA = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image A"));
        var imagePortB = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image B"));
        var resultImagePort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image"));
        var operationPort = (EnumPort)_plugin.Ports.First(p => p.Name.Equals("Operation"));

        imagePortA.Value = testImageA;
        imagePortB.Value = testImageB;
        operationPort.Value = mode;

        // Act
        bool result = await _plugin.TryRunAsync(default);

        // Assert
        Assert.True(result);
        Assert.Equal(testImageA.PixelFormat, resultImagePort.Value.PixelFormat);
        Assert.Equal(testImageA.Width, resultImagePort.Value.Width);
        Assert.Equal(testImageA.Height, resultImagePort.Value.Height);

        string hash = Hash.Create(resultImagePort.Value.AsPacked<Rgb24>().Pixels[..1000].AsByte());
        Assert.Equal(expectedResult, hash);
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
