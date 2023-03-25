using System.Collections.ObjectModel;
using System.Text.Json;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Data.Mapper;

public class RectangleCollectionPortMapper : IPortMapper<ReadOnlyCollection<Rectangle>>
{
    public SDK.Common.Models.Port ToModel(IPort port)
    {
        var typedPort = (RectangleCollectionPort)port;
        return new SDK.Common.Models.Port
        {
            Id = port.Id,
            Name = port.Name,
            Direction = port.Direction,
            Brand = port.Brand,
            IsConnected = port.IsConnected,
            IsLinkConvertable = typedPort.IsLinkConvertable,
            Value = JsonSerializer.Serialize(typedPort.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        };
    }
    public ReadOnlyCollection<Rectangle> ToNativeValue(object value, Type? type = null)
    {
        List<Rectangle> record;
        if (value is ReadOnlyCollection<SDK.Common.Models.Rectangle> collection)
        {
            record = new List<Rectangle>();
            foreach (SDK.Common.Models.Rectangle rec in collection)
            {
                record.Add(new Rectangle { X = rec.X, Y = rec.Y, Width = rec.Width, Height = rec.Height });
            }
        }
        else
        {
            record = JsonSerializer.Deserialize<List<Rectangle>>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        return new ReadOnlyCollection<Rectangle>(record);
    }
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public void Update(IPort port, object value) => ((RectangleCollectionPort)port).Value = ToNativeValue(value);
}
