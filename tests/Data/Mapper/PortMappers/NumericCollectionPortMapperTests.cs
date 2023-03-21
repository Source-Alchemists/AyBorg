using System.Collections.ObjectModel;
using System.Text.Json;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper.Tests;

public class NumericCollectionPortMapperTests
{
    [Fact]
    public void Test_ToModel()
    {
        // Arrange
        var mapper = new NumericCollectionPortMapper();
        var port = new NumericCollectionPort("Test", PortDirection.Input, new ReadOnlyCollection<double>(new List<double> { 1, 2 }));

        // Act
        SDK.Common.Models.Port portModel = mapper.ToModel(port);

        // Assert
        Assert.Equal(port.Name, portModel.Name);
        Assert.Equal(port.Direction, portModel.Direction);
        Assert.Equal(JsonSerializer.Serialize(port.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }), portModel.Value);
    }
}
