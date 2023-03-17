using System.Collections.ObjectModel;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Plugins.Base.Collection.Tests;

public class StringCollectionIndexOfTests {
    private static readonly NullLogger<StringCollectionIndexOf> s_nullLogger = new();

    [Fact]
    public async Task Test_TryRunAsync_Success()
    {
        // Arrange
        var plugin = new StringCollectionIndexOf(s_nullLogger);
        var searchValuePort = (StringPort)plugin.Ports.First(p => p.Name.Equals("Value"));
        var collectionPort = (StringCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));

        collectionPort.Value = new ReadOnlyCollection<string>(new List<string>{"Test1", "Test2"});
        searchValuePort.Value = "Test2";

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
        var plugin = new StringCollectionIndexOf(s_nullLogger);
        var searchValuePort = (StringPort)plugin.Ports.First(p => p.Name.Equals("Value"));
        var collectionPort = (StringCollectionPort)plugin.Ports.First(p => p.Name.Equals("Collection"));
        var resultPort = (NumericPort)plugin.Ports.First(p => p.Name.Equals("Index"));

        collectionPort.Value = new ReadOnlyCollection<string>(new List<string>{"Test1", "Test2"});
        searchValuePort.Value = "Test3";

        // Act
        bool result = await plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.Equal(-1, resultPort.Value);
    }
}
