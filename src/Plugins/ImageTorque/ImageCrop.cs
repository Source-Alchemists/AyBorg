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

using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque;

public sealed class ImageCrop : IStepBody, IDisposable
{
    private readonly ILogger<ImageCrop> _logger;
    private readonly ImagePort _inputImagePort = new("Image", PortDirection.Input, null!);
    private readonly RectanglePort _cropRectanglePort = new("Region", PortDirection.Input, new Rectangle());
    private readonly ImagePort _outputImagePort = new("Cropped image", PortDirection.Output, null!);
    private bool _disposedValue;

    public string Name => "Image.Crop";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

    public IReadOnlyCollection<IPort> Ports { get; }

    public ImageCrop(ILogger<ImageCrop> logger)
    {
        _logger = logger;
        Ports = new IPort[]
        {
            _inputImagePort,
            _cropRectanglePort,
            _outputImagePort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _outputImagePort.Value?.Dispose();
        if (_cropRectanglePort.Value.Width <= 0 || _cropRectanglePort.Value.Height <= 0)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Result), "Invalid crop region.");
            return ValueTask.FromResult(false);
        }

        _outputImagePort.Value = _inputImagePort.Value.Crop(_cropRectanglePort.Value);
        return ValueTask.FromResult(true);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            _outputImagePort.Value?.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
