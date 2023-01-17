using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class ImageBinarizeTests : IDisposable
{
    private readonly ImageBinarize _plugin = new();
    private bool _disposedValue;

    [Fact]
    public async ValueTask Test_TryRunAsync()
    {
        // Arrange
        using Image testImage = Image.Load("./resources/luna.jpg");
        var imageInputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image"));
        imageInputPort.Value = testImage;
        var imageOutputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Binarized image"));

        // Act
        bool result = await _plugin.TryRunAsync(default);

        // Assert
        Assert.True(result);
        Assert.Equal(PixelFormat.Mono8, imageOutputPort.Value.PixelFormat);
        Assert.Equal(testImage.Width, imageOutputPort.Value.Width);
        Assert.Equal(testImage.Height, imageOutputPort.Value.Height);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            _plugin.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
