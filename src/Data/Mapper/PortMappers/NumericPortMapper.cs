using System.Globalization;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public sealed class NumericPortMapper : IPortMapper<double>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public double ToNativeValue(object value, Type? type = null) => Convert.ToDouble(value, CultureInfo.InvariantCulture);
    public void Update(IPort port, object value) => ((NumericPort)port).Value = ToNativeValue(value);
    public Port ToModel(IPort port)
    {
        var typedPort = (NumericPort)port;
        Port record = GenericPortMapper.ToRecord(typedPort);
        record.IsLinkConvertable = typedPort.IsLinkConvertable;
        record.Value = typedPort.Value.ToString(CultureInfo.InvariantCulture);
        return record;
    }
}
