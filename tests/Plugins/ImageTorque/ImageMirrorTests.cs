using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Plugins.ImageTorque.Tests;

public class ImageMirrorTests : IDisposable
{
    private readonly ImageMirror _plugin = new();
    private bool _disposedValue;

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Test_TryRunAsync(bool mirrorVertical, bool mirrorHorizontal)
    {
        // Arrange
        using Image testImage = Image.Load("./resources/luna.jpg");
        var imageInputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image"));
        imageInputPort.Value = testImage;
        var imageOutputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Mirrored image"));
        var mirrorVerticalPort = (BooleanPort)_plugin.Ports.First(p => p.Name.Equals("Vertical"));
        var mirrorHorizonzalPort = (BooleanPort)_plugin.Ports.First(p => p.Name.Equals("Horizontal"));
        mirrorVerticalPort.Value = mirrorVertical;
        mirrorHorizonzalPort.Value = mirrorHorizontal;

        // Act
        bool result = await _plugin.TryRunAsync(default);

        // Assert
        Assert.True(result);
        Assert.Equal(testImage.PixelFormat, imageOutputPort.Value.PixelFormat);
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
