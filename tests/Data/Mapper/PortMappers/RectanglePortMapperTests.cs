using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper.Tests;

public class RectanglePortMapperTests
{
    [Fact]
    public void Test_ToModel()
    {
        // Arrange
        var mapper = new RectanglePortMapper();
        var port = new RectanglePort("Test", PortDirection.Input, new ImageTorque.Rectangle {
            X = 1,
            Y = 2,
            Width = 3,
            Height = 4
        });

        // Act
        SDK.Common.Models.Port portModel = mapper.ToModel(port);

        // Assert
        Assert.Equal(port.Name, portModel.Name);
        Assert.Equal(port.Direction, portModel.Direction);
        Assert.Equal(port.Value, portModel.Value);
    }
}
