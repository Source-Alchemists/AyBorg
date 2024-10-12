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

using AyBorg.Types;
using AyBorg.Types.Ports;

using ImageTorque;

namespace AyBorg.Plugins.ImageTorque;

public sealed class ImageScale : IStepBody, IDisposable
{
    private readonly ImagePort _imagePort = new("Image", PortDirection.Input, null!);
    private readonly ImagePort _scaledImagePort = new("Scaled image", PortDirection.Output, null!);
    private readonly NumericPort _widthPort = new("Width", PortDirection.Output, 0);
    private readonly NumericPort _heightPort = new("Height", PortDirection.Output, 0);
    private readonly NumericPort _scalePort = new("Scale factor", PortDirection.Input, 0.5d, 0.01d, 2d);
    private bool _disposedValue;

    public string Name => "Image.Scale";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

    public IReadOnlyCollection<IPort> Ports { get; }

    public ImageScale()
    {
        Ports = new List<IPort>
        {
            _imagePort,
            _scaledImagePort,
            _widthPort,
            _heightPort,
            _scalePort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _scaledImagePort.Value?.Dispose();
        Image sourceImage = _imagePort.Value;
        if (_scalePort.Value.Equals(1d))
        {
            _scaledImagePort.Value = sourceImage;
            return ValueTask.FromResult(true);
        }

        int w = (int)(sourceImage.Width * _scalePort.Value);
        int h = (int)(sourceImage.Height * _scalePort.Value);
        _scaledImagePort.Value = sourceImage.Resize(w, h);
        _widthPort.Value = w;
        _heightPort.Value = h;
        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            _scaledImagePort?.Dispose();
            _disposedValue = true;
        }
    }
}
