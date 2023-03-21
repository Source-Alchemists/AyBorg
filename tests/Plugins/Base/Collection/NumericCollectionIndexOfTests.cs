using System.Collections.ObjectModel;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Plugins.Base.Collection.Tests;

public class NumericCollectionIndexOfTests
{
    private static readonly NullLogger<NumericCollectionIndexOf> s_nullLogger = new();

    [Fact]
    public async Task Test_TryRunAsync_Success()
    {
        // Arrange
        var plugin = new NumericCollectionIndexOf(s_nullLogger);
        var searchValuePort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Value"));
        var collectionPort = (NumericCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));

        collectionPort.Value = new ReadOnlyCollection<double>(new List<double> { 1, 2 });
        searchValuePort.Value = 2;

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(1, resultPort.Value);
    }

    [Fact]
    public async Task Test_TryRunAsync_Failed()
    {
        // Arrange
        var plugin = new NumericCollectionIndexOf(s_nullLogger);
        var searchValuePort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Value"));
        var collectionPort = (NumericCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));

        collectionPort.Value = new ReadOnlyCollection<double>(new List<double> { 1, 2 });
        searchValuePort.Value = 3;

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.Equal(-1, resultPort.Value);
    }
}
