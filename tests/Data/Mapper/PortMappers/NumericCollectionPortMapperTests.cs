using System.Collections.Immutable;
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
        var port = new NumericCollectionPort("Test", PortDirection.Input, new List<double> { 1, 2 }.ToImmutableList());

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
        var port = new NumericCollectionPort("Test", PortDirection.Input, new List<double> { 1, 2 }.ToImmutableList());

        // Act
        ImmutableList<double> nativeValue = mapper.ToNativeValue(port.Value);

        // Assert
        Assert.Equal(port.Value, nativeValue);
    }

    [Fact]
    public void Test_ToNative_FromJson()
    {
        // Arrange
        var mapper = new NumericCollectionPortMapper();
        var port = new NumericCollectionPort("Test", PortDirection.Input, new List<double> { 1, 2 }.ToImmutableList());
        string json = JsonSerializer.Serialize(port.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Act
        ImmutableList<double> nativeValue = mapper.ToNativeValue(json);

        // Assert
        Assert.Equal(new List<double> { 1, 2 }.ToImmutableList(), nativeValue);
    }
}
