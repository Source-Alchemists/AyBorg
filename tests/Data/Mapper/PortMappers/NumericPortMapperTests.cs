using System.Globalization;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper.Tests;

public class NumericPortMapperTests
{
    [Fact]
    public void Test_ToModel()
    {
        // Arrange
        var mapper = new NumericPortMapper();
        var port = new NumericPort("Test", PortDirection.Input, 123);

        // Act
        SDK.Common.Models.Port portModel = mapper.ToModel(port);

        // Assert
        Assert.Equal(port.Name, portModel.Name);
        Assert.Equal(port.Direction, portModel.Direction);
        Assert.Equal(port.Value.ToString(CultureInfo.InvariantCulture), portModel.Value);
    }
}
