using System.Globalization;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public class StringPortMapper : IPortMapper<string>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public string ToNativeValue(object value, Type? type = null) => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    public void Update(IPort port, object value) => ((StringPort)port).Value = ToNativeValue(value);
    public Port ToModel(IPort port)
    {
        var typedPort = (StringPort)port;
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
