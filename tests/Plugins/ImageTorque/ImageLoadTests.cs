using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class ImageLoadTests : IDisposable
{
    private static readonly NullLogger<ImageLoad> s_logger = new();
    private readonly Mock<IEnvironment> _mockEnvironment = new();
    private readonly ImageLoad _plugin;
    private bool _disposedValue;

    public ImageLoadTests()
    {
        _plugin = new ImageLoad(s_logger, _mockEnvironment.Object);
    }

    [Fact]
    public async Task Test_TryRunAsync()
    {
        // Arrange
        _mockEnvironment.Setup(m => m.StorageLocation).Returns("./");
        var folderPort = (FolderPort)_plugin.Ports.First(p => p.Name.Equals("Folder"));
        var imagePort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image"));
        folderPort.Value = "resources";

        // Act
        bool result = await _plugin.TryRunAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.NotNull(imagePort.Value);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            Task.Delay(10).Wait(); // Give some time to finished the background task.
            _plugin?.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
