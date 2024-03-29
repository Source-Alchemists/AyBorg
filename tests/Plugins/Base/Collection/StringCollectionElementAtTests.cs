using System.Collections.Immutable;
using AyBorg.SDK.Common.Ports;
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
