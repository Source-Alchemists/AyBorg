using System.Text.Json;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Data.Mapper;

public sealed class RectanglePortMapper : IPortMapper<ImageTorque.Rectangle>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValueObject(value);
    public ImageTorque.Rectangle ToNativeValue(object value, Type? type = null)
    {
        if (value is ImageTorque.Rectangle rectangle)
        {
            return rectangle;
        }

        if (value is SDK.Common.Models.Rectangle rectangleModel)
        {
            return new ImageTorque.Rectangle(rectangleModel.X, rectangleModel.Y, rectangleModel.Width, rectangleModel.Height);
        }

        RectangleRecord record = JsonSerializer.Deserialize<RectangleRecord>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        return new ImageTorque.Rectangle(record.X, record.Y, record.Width, record.Height);
    }
    public void Update(IPort port, object value) => ((RectanglePort)port).Value = ToNativeValue(value);
    public Port ToRecord(IPort port)
    {
        var typedPort = (RectanglePort)port;
        Port record = GenericPortMapper.ToRecord(typedPort);
        record.IsLinkConvertable = typedPort.IsLinkConvertable;
        record.Value = typedPort.Value;
        return record;
    }
}
