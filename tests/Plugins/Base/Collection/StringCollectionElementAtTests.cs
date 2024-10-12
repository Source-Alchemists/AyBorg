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
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Plugins.Base.Collection.Tests;

public class StringCollectionElementAtTests
{
    private static readonly NullLogger<StringCollectionElementAt> s_nullLogger = new();

    [Fact]
    public async Task Test_TryRunAsync_Success()
    {
        // Arrange
        var plugin = new StringCollectionElementAt(s_nullLogger);
        var indexPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));
        var collectionPort = (StringCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (StringPort)plugin.Ports.First(p => p.Name.Equals("Result"));

        collectionPort.Value = new List<string> { "Test1", "Test2" }.ToImmutableList();
        indexPort.Value = 1;

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("Test2", resultPort.Value);
    }

    [Fact]
    public async Task Test_TryRunAsync_OutOfRange()
    {
        // Arrange
        var plugin = new StringCollectionElementAt(s_nullLogger);
        var indexPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));
        var collectionPort = (StringCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (StringPort)plugin.Ports.First(p => p.Name.Equals("Result"));

        collectionPort.Value = new List<string> { "Test1", "Test2" }.ToImmutableList();
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
        var plugin = new StringCollectionElementAt(s_nullLogger);
        var indexPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));
        var collectionPort = (StringCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (StringPort)plugin.Ports.First(p => p.Name.Equals("Result"));

        collectionPort.Value = new List<string> { "Test1", "Test2" }.ToImmutableList();
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
        var plugin = new StringCollectionElementAt(s_nullLogger);
        var indexPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));
        var collectionPort = (StringCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (StringPort)plugin.Ports.First(p => p.Name.Equals("Result"));

        collectionPort.Value = ImmutableList<string>.Empty;

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}
