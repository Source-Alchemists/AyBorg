using System.Text.Json;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Data.Mapper;

public sealed class RectanglePortMapper : IPortMapper<Rectangle>
{
    public object ToNativeObject(object value, Type? type = null) => ToNativeObject(value);
    public Rectangle ToNativeType(object value, Type? type = null)
    {
        if (value is Rectangle rectangle)
        {
            return rectangle;
        }

        if (value is SDK.Common.Models.Rectangle rectangleModel)
        {
            return new Rectangle(rectangleModel.X, rectangleModel.Y, rectangleModel.Width, rectangleModel.Height);
        }

        RectangleRecord record = JsonSerializer.Deserialize<RectangleRecord>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        return new Rectangle(record.X, record.Y, record.Width, record.Height);
    }
    public void Update(IPort port, object value) => ((RectanglePort)port).Value = ToNativeType(value);
}
