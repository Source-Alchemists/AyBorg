using System.Collections.ObjectModel;
using AyBorg.SDK.Common.Ports;
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

        collectionPort.Value = new ReadOnlyCollection<Rectangle>(new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4 },
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8 } });
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

        collectionPort.Value = new ReadOnlyCollection<Rectangle>(new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4 },
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8 } });
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

        collectionPort.Value = new ReadOnlyCollection<Rectangle>(new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4 },
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8 } });
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

        collectionPort.Value = new ReadOnlyCollection<Rectangle>(Array.Empty<Rectangle>());

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}

