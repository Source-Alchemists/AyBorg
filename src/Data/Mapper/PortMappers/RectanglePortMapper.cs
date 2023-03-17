using System.Text.Json;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public sealed class RectanglePortMapper : IPortMapper<ImageTorque.Rectangle>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public ImageTorque.Rectangle ToNativeValue(object value, Type? type = null)
    {
        if (value is ImageTorque.Rectangle rectangle)
        {
            return rectangle;
        }

        if (value is Rectangle rectangleModel)
        {
            return new ImageTorque.Rectangle(rectangleModel.X, rectangleModel.Y, rectangleModel.Width, rectangleModel.Height);
        }

        RectangleRecord record = JsonSerializer.Deserialize<RectangleRecord>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        return new ImageTorque.Rectangle(record.X, record.Y, record.Width, record.Height);
    }
    public void Update(IPort port, object value) => ((RectanglePort)port).Value = ToNativeValue(value);
    public Port ToModel(IPort port)
    {
        var typedPort = (RectanglePort)port;
        return new Port
        {
            Id = port.Id,
            Name = port.Name,
            Direction = port.Direction,
            Brand = port.Brand,
            IsConnected = port.IsConnected,
            IsLinkConvertable = typedPort.IsLinkConvertable,
            Value = typedPort.Value
        };
    }
}
