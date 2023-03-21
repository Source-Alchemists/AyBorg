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

    [Fact]
    public void Test_ToNative()
    {
        // Arrange
        var mapper = new NumericCollectionPortMapper();
        var port = new NumericCollectionPort("Test", PortDirection.Input, new ReadOnlyCollection<double>(new List<double> { 1, 2 }));

        // Act
        ReadOnlyCollection<double> nativeValue = mapper.ToNativeValue(port.Value);

        // Assert
        Assert.Equal(port.Value, nativeValue);
    }

    [Fact]
    public void Test_ToNative_FromJson()
    {
        // Arrange
        var mapper = new NumericCollectionPortMapper();
        var port = new NumericCollectionPort("Test", PortDirection.Input, new ReadOnlyCollection<double>(new List<double> { 1, 2 }));
        string json = JsonSerializer.Serialize(port.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Act
        ReadOnlyCollection<double> nativeValue = mapper.ToNativeValue(json);

        // Assert
        Assert.Equal(new ReadOnlyCollection<double>(new List<double> { 1, 2 }), nativeValue);
    }
}
