using System.Collections.ObjectModel;
using System.Text.Json;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper.Tests;

public class StringCollectionPortMapperTests
{
    [Fact]
    public void Test_ToModel()
    {
        // Arrange
        var mapper = new StringCollectionPortMapper();
        var port = new StringCollectionPort("Test", PortDirection.Input, new ReadOnlyCollection<string>(new List<string> { "Test1", "Test2" }));

        // Act
        SDK.Common.Models.Port portModel = mapper.ToModel(port);

        // Assert
        Assert.Equal(port.Name, portModel.Name);
        Assert.Equal(port.Direction, portModel.Direction);
        Assert.Equal(JsonSerializer.Serialize(port.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }), portModel.Value);
    }
}
