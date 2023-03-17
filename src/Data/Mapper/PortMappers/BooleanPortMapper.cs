using System.Globalization;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public class BooleanPortMapper : IPortMapper<bool>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public bool ToNativeValue(object value, Type? type = null)  => Convert.ToBoolean(value, CultureInfo.InvariantCulture);
    public void Update(IPort port, object value) => ((BooleanPort)port).Value = ToNativeValue(value);
    public Port ToModel(IPort port)
    {
        var typedPort = (BooleanPort)port;
        Port record = GenericPortMapper.ToRecord(typedPort);
        record.IsLinkConvertable = typedPort.IsLinkConvertable;
        record.Value = typedPort.Value;
        return record;
    }
}
