using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper.Tests;

public class BooleanPortMapperTests
{
    [Fact]
    public void Test_ToModel()
    {
        // Arrange
        var mapper = new BooleanPortMapper();
        var port = new BooleanPort("Test", PortDirection.Input, true);

        // Act
        SDK.Common.Models.Port portModel = mapper.ToModel(port);

        // Assert
        Assert.Equal(port.Name, portModel.Name);
        Assert.Equal(port.Direction, portModel.Direction);
        Assert.Equal(port.Value, portModel.Value);
    }
}
