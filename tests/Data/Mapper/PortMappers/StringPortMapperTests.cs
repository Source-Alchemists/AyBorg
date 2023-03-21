using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper.Tests;

public class StringPortMapperTests
{
    [Fact]
    public void Test_ToModel()
    {
        // Arrange
        var mapper = new StringPortMapper();
        var port = new StringPort("Test", PortDirection.Input, "Test123");

        // Act
        SDK.Common.Models.Port portModel = mapper.ToModel(port);

        // Assert
        Assert.Equal(port.Name, portModel.Name);
        Assert.Equal(port.Direction, portModel.Direction);
        Assert.Equal(port.Value, portModel.Value);
    }
}
