using System.Collections.ObjectModel;
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
        var port = new RectangleCollectionPort("Test", PortDirection.Input, new ReadOnlyCollection<Rectangle>(new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4},
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8}
         }));

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
        var port = new RectangleCollectionPort("Test", PortDirection.Input, new ReadOnlyCollection<Rectangle>(new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4},
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8}
         }));

        // Act
        ReadOnlyCollection<Rectangle> nativeValue = mapper.ToNativeValue(port.Value);

        // Assert
        Assert.Equal(port.Value, nativeValue);
    }

    [Fact]
    public void Test_ToNative_FromJson()
    {
        // Arrange
        var mapper = new RectangleCollectionPortMapper();
        var port = new RectangleCollectionPort("Test", PortDirection.Input, new ReadOnlyCollection<Rectangle>(new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4},
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8}
         }));
        string json = JsonSerializer.Serialize(port.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Act
        ReadOnlyCollection<Rectangle> nativeValue = mapper.ToNativeValue(json);

        // Assert
        Assert.Equal(new ReadOnlyCollection<Rectangle>(new List<Rectangle> {
            new Rectangle { X = 1, Y = 2, Width = 3, Height = 4},
            new Rectangle { X = 5, Y = 6, Width = 7, Height = 8}
        }), nativeValue);
    }
}
