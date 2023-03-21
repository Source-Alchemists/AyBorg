using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper.Tests;

public class EnumPortMapperTests
{
    [Fact]
    public void Test_ToModel()
    {
        // Arrange
        var mapper = new EnumPortMapper();
        var port = new EnumPort("Test", PortDirection.Input, TestEnum.B);

        // Act
        SDK.Common.Models.Port portModel = mapper.ToModel(port);

        // Assert
        Assert.Equal(port.Name, portModel.Name);
        Assert.Equal(port.Direction, portModel.Direction);
        Assert.Equal(port.Value, portModel.Value);
    }

    public enum TestEnum {
        A,
        B,
        C
    }
}
