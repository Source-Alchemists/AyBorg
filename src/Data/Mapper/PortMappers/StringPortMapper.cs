using System.Globalization;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public class StringPortMapper : IPortMapper<string>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public string ToNativeValue(object value, Type? type = null) => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    public void Update(IPort port, object value) => ((StringPort)port).Value = ToNativeValue(value);
    public Port ToRecord(IPort port)
    {
        var typedPort = (StringPort)port;
        Port record = GenericPortMapper.ToRecord(typedPort);
        record.IsLinkConvertable = typedPort.IsLinkConvertable;
        record.Value = typedPort.Value;
        return record;
    }
}
