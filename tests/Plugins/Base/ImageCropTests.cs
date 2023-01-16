using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging.Abstractions;

namespace AyBorg.Plugins.Base.Tests;

public class ImageCropTests : IDisposable
{
    private static readonly NullLogger<ImageCrop> s_logger = new();
    private readonly ImageCrop _plugin = new(s_logger);
    private bool _disposedValue;

    [Theory]
    [InlineData(true, 10, 10)]
    [InlineData(false, 0, 10)]
    [InlineData(false, 10, 0)]
    public async ValueTask Test_TryRunAsync(bool expectedResult, int width, int height)
    {
        // Arrange
        using Image testImage = Image.Load("./resources/luna.jpg");
        var imageInputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Image"));
        imageInputPort.Value = testImage;
        var imageOutputPort = (ImagePort)_plugin.Ports.First(p => p.Name.Equals("Cropped image"));
        var regionPort = (RectanglePort)_plugin.Ports.First(p => p.Name.Equals("Region"));
        regionPort.Value = new Rectangle(0, 0, width, height);

        // Act
        bool result = await _plugin.TryRunAsync(default);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.Equal(width, imageOutputPort.Value.Width);
            Assert.Equal(height, imageOutputPort.Value.Height);
        }
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
