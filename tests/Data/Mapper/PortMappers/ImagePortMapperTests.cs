using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using ImageTorque.Buffers;
using ImageTorque.Pixels;

namespace AyBorg.Data.Mapper.Tests;

public class ImagePortMapperTests
{
    [Fact]
    public void Test_ToModel()
    {
        // Arrange
        var mapper = new ImagePortMapper();
        var pixelBuffer = new PixelBuffer<L8>(4, 4);
        var image = new ImageTorque.Image(pixelBuffer);
        var port = new ImagePort("Test", PortDirection.Input, image);

        // Act
        Port portModel = mapper.ToModel(port);

        // Assert
        Assert.Equal(port.Name, portModel.Name);
        Assert.Equal(port.Direction, portModel.Direction);
        Assert.Equal(new CacheImage {
            OriginalImage = image,
            Meta = new ImageMeta {
                Width = image.Width,
                Height = image.Height,
                PixelFormat = ImageTorque.PixelFormat.Mono8
            }
        }, portModel.Value);
    }
}
