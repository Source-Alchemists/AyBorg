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

using System.Globalization;

namespace AyBorg.Plugins.ImageTorque;

public sealed class ImageBinarize : IStepBody, IDisposable
{
    private readonly ImagePort _inputImagePort = new("Image", PortDirection.Input, null!);
    private readonly NumericPort _thresholdPort = new("Threshold", PortDirection.Input, 0.5d, 0d, 1d);
    private readonly ImagePort _outputImagePort = new("Binarized image", PortDirection.Output, null!);
    private readonly EnumPort _thresholdTypePort = new("Mode", PortDirection.Input, BinaryThresholdMode.Lumincance);
    private bool _disposedValue;

    public string Name => "Image.Binarize";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

    public IReadOnlyCollection<IPort> Ports { get; }

    public ImageBinarize()
    {
        Ports = new IPort[]
        {
            _inputImagePort,
            _thresholdTypePort,
            _thresholdPort,
            _outputImagePort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        float threshold = Convert.ToSingle(_thresholdPort.Value, CultureInfo.InvariantCulture);
        var mode = (BinaryThresholdMode)_thresholdTypePort.Value;
        _outputImagePort.Value?.Dispose();
        _outputImagePort.Value = _inputImagePort.Value.Binarize(threshold, mode);

        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _outputImagePort.Dispose();
            }
            _disposedValue = true;
        }
    }
}
