using AyBorg.SDK.Common.Ports;

namespace AyBorg.Plugins.Base.Tests;

public class ShapeRectangleCreateTests
{
    private readonly ShapeRectangleCreate _plugin = new();

    [Fact]
    public async ValueTask Test_TryRunAsync()
    {
        // Arrange
        var xPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("X"));
        var yPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Y"));
        var widthPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Width"));
        var heightPort = (NumericPort)_plugin.Ports.First(p => p.Name.Equals("Height"));
        var rectanglePort = (RectanglePort)_plugin.Ports.First(p => p.Name.Equals("Rectangle"));
        xPort.Value = 1;
        yPort.Value = 2;
        widthPort.Value = 3;
        heightPort.Value = 4;

        // Act
        bool result = await _plugin.TryRunAsync(default);

        // Assert
        Assert.True(result);
        Assert.Equal(xPort.Value, rectanglePort.Value.X);
        Assert.Equal(yPort.Value, rectanglePort.Value.Y);
        Assert.Equal(widthPort.Value, rectanglePort.Value.Width);
        Assert.Equal(heightPort.Value, rectanglePort.Value.Height);
    }
}
