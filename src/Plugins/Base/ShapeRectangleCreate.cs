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

namespace AyBorg.Plugins.Base;

public sealed class ShapeRectangleCreate : IStepBody
{
    private readonly NumericPort _xPort = new("X", PortDirection.Input, 0d, 0d);
    private readonly NumericPort _yPort = new("Y", PortDirection.Input, 0d, 0d);
    private readonly NumericPort _widthPort = new("Width", PortDirection.Input, 1d, 1d);
    private readonly NumericPort _heightPort = new("Height", PortDirection.Input, 1d, 1d);
    private readonly RectanglePort _rectanglePort = new("Rectangle", PortDirection.Output, new Rectangle());

    public string Name => "Shape.Rectangle.Create";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing, DefaultStepCategories.ImageShapes };

    public IReadOnlyCollection<IPort> Ports { get; }

    public ShapeRectangleCreate()
    {
        Ports = new List<IPort>
        {
            _xPort,
            _yPort,
            _widthPort,
            _heightPort,
            _rectanglePort
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _rectanglePort.Value = new Rectangle(Convert.ToInt32(_xPort.Value),
                                                Convert.ToInt32(_yPort.Value),
                                                Convert.ToInt32(_widthPort.Value),
                                                Convert.ToInt32(_heightPort.Value));
        return ValueTask.FromResult(true);
    }
}
