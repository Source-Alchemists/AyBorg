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

using System.Collections.Immutable;
using AyBorg.Types.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Plugins.Base.Collection.Tests;

public class RectangleCollectionElementAtTests
{
    private static readonly NullLogger<RectangleCollectionElementAt> s_nullLogger = new();

    [Fact]
    public async Task Test_TryRunAsync_Success()
    {
        // Arrange
        var plugin = new RectangleCollectionElementAt(s_nullLogger);
        var indexPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));
        var collectionPort = (RectangleCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (RectanglePort)plugin.Ports.First(p => p.Name.Equals("Result"));

        collectionPort.Value = new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4 },
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8 } }.ToImmutableList();
        indexPort.Value = 1;

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(new Rectangle { X = 5, Y = 6, Width = 7, Height = 8 }, resultPort.Value);
    }

    [Fact]
    public async Task Test_TryRunAsync_OutOfRange()
    {
        // Arrange
        var plugin = new RectangleCollectionElementAt(s_nullLogger);
        var indexPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));
        var collectionPort = (RectangleCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (RectanglePort)plugin.Ports.First(p => p.Name.Equals("Result"));

        collectionPort.Value = new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4 },
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8 } }.ToImmutableList();
        indexPort.Value = 2;

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Test_TryRunAsync_Overflow()
    {
        // Arrange
        var plugin = new RectangleCollectionElementAt(s_nullLogger);
        var indexPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));
        var collectionPort = (RectangleCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (RectanglePort)plugin.Ports.First(p => p.Name.Equals("Result"));

        collectionPort.Value = new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4 },
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8 } }.ToImmutableList();
        indexPort.Value = double.MaxValue;

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Test_TryRunAsync_Empty()
    {
        // Arrange
        var plugin = new RectangleCollectionElementAt(s_nullLogger);
        var indexPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));
        var collectionPort = (RectangleCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (RectanglePort)plugin.Ports.First(p => p.Name.Equals("Result"));

        collectionPort.Value = ImmutableList<Rectangle>.Empty;

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}

