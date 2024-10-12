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

namespace AyBorg.Plugins.ImageTorque.Display;

public sealed class ImageObjectsDisplay : IStepBody
{
    private readonly ImagePort _imagePort = new("Image", PortDirection.Input);
    private readonly RectangleCollectionPort _regionsPort = new("Regions", PortDirection.Input);
    private readonly StringCollectionPort _labelsPort = new("Labels", PortDirection.Input);
    private readonly NumericCollectionPort _scoresPort = new("Scores", PortDirection.Input);
    public string Name => "Image.Objects.Display";

    public IReadOnlyCollection<string> Categories => new List<string> { DefaultStepCategories.Display, DefaultStepCategories.Ai };

    public IReadOnlyCollection<IPort> Ports { get; }

    public ImageObjectsDisplay()
    {
        Ports = new List<IPort>
        {
            _imagePort,
            _regionsPort,
            _labelsPort,
            _scoresPort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken) => ValueTask.FromResult(true);
}
