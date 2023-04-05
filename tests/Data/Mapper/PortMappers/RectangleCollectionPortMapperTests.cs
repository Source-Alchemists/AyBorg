using System.Collections.Immutable;
using System.Text.Json;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Data.Mapper.Tests;

public class RectangleCollectionPortMapperTests
{
    [Fact]
    public void Test_ToModel()
    {
        // Arrange
        var mapper = new RectangleCollectionPortMapper();
        var port = new RectangleCollectionPort("Test", PortDirection.Input, new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4},
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8}
         }.ToImmutableList());

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
        var mapper = new RectangleCollectionPortMapper();
        var value = new List<SDK.Common.Models.Rectangle> {
            new SDK.Common.Models.Rectangle { X = 1, Y = 2, Width = 3, Height = 4},
            new SDK.Common.Models.Rectangle { X = 5, Y = 6, Width = 7, Height = 8}
         }.ToImmutableList();

        // Act
        ImmutableList<Rectangle> nativeValue = mapper.ToNativeValue(value);

        // Assert
        Assert.Equal(value.Count, nativeValue.Count);
        int index = 0;
        foreach(Rectangle n in nativeValue)
        {
            SDK.Common.Models.Rectangle v = value.ElementAt(index);
            Assert.Equal(v.X, n.X);
            Assert.Equal(v.Y, n.Y);
            Assert.Equal(v.Width, n.Width);
            Assert.Equal(v.Height, n.Height);
            index++;
        }
    }

    [Fact]
    public void Test_ToNative_FromJson()
    {
        // Arrange
        var mapper = new RectangleCollectionPortMapper();
        var port = new RectangleCollectionPort("Test", PortDirection.Input, new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4},
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8}
         }.ToImmutableList());
        string json = JsonSerializer.Serialize(port.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Act
        ImmutableList<Rectangle> nativeValue = mapper.ToNativeValue(json);

        // Assert
        Assert.Equal(new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4},
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8}
        }.ToImmutableList(), nativeValue);
    }
}
