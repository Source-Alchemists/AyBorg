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
        return new Port
        {
            Id = port.Id,
            Name = port.Name,
            Direction = port.Direction,
            Brand = port.Brand,
            IsConnected = port.IsConnected,
            IsLinkConvertable = typedPort.IsLinkConvertable,
            Value = typedPort.Value.ToString(CultureInfo.InvariantCulture)
        };
    }
}
