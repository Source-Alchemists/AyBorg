using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class ImageScaleTests : IDisposable
{
    private readonly ImageScale _plugin = new();
    private bool _disposedValue;

    [Theory]
    [InlineData(409, 202, 0.5d)]
    [InlineData(819, 404, 1d)]
    public async ValueTask Test_TryRunAsync(int expectedWidth, int expectedHeight, double scaleFactor)
    {
        // Arrange
        using Image testImage = Image.Load("./resources/luna.jpg");
        var imageInputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image"));
        imageInputPort.Value = testImage;
        var imageOutputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Scaled image"));
        var widthPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Width"));
        var heightPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Height"));
        var scalePort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Scale factor"));
        scalePort.Value = scaleFactor;

        // Act
        bool result = await _plugin.TryRunAsync(default);

        // Assert
        Assert.True(result);
        Assert.Equal(testImage.PixelFormat, imageOutputPort.Value.PixelFormat);
        Assert.Equal(expectedWidth, imageOutputPort.Value.Width);
        Assert.Equal(expectedHeight, imageOutputPort.Value.Height);
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
