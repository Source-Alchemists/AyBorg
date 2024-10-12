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

public sealed class ImageMirror : IStepBody, IDisposable
{
    private readonly BooleanPort _mirrorVertical = new("Vertical", PortDirection.Input, false);
    private readonly BooleanPort _mirrorHorizontal = new("Horizontal", PortDirection.Input, false);
    private readonly ImagePort _inputImage = new("Image", PortDirection.Input, null!);
    private readonly ImagePort _outputImage = new("Mirrored image", PortDirection.Output, null!);
    private bool _disposedValue;

    public string Name => "Image.Mirror";

    public IReadOnlyCollection<string> Categories { get; } = [DefaultStepCategories.ImageProcessing];

    public IReadOnlyCollection<IPort> Ports { get; }

    public ImageMirror()
    {
        Ports = new IPort[]
        {
            _inputImage,
            _mirrorHorizontal,
            _mirrorVertical,
            _outputImage
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _outputImage.Value?.Dispose();

        if (_mirrorHorizontal.Value == false && _mirrorVertical.Value == false)
        {
            // No mirror, just copy the input to the output
            _outputImage.Value = new Image(_inputImage.Value);
            return ValueTask.FromResult(true);
        }

        MirrorMode mirrorMode = MirrorMode.Horizontal;
        if (_mirrorHorizontal.Value && _mirrorVertical.Value)
        {
            mirrorMode = MirrorMode.VerticalHorizontal;
        }
        else if (_mirrorVertical.Value)
        {
            mirrorMode = MirrorMode.Vertical;
        }

        _outputImage.Value = new Image(_inputImage.Value.Mirror(mirrorMode));

        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _outputImage.Dispose();
            }
            _disposedValue = true;
        }
    }
}
