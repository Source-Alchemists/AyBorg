using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper.Tests;

public class SelectPortMapperTests
{
    [Fact]
    public void Test_ToModel()
    {
        // Arrange
        var mapper = new SelectPortMapper();
        var port = new SelectPort("TestSelect", PortDirection.Input, new SelectPort.ValueContainer("TestValue", new List<string> { "TestValue", "SecondValue" }));

        // Act
        Port portModel = mapper.ToModel(port);

        // Assert
        Assert.Equal(port.Name, portModel.Name);
        Assert.Equal(port.Direction, portModel.Direction);
        Assert.Equal(port.Value, portModel.Value);
    }
}
