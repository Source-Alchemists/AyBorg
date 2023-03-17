using System.Globalization;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public sealed class NumericPortMapper : IPortMapper<double>
{
    public object ToNativeObject(object value, Type? type = null) => ToNativeObject(value);
    public double ToNativeType(object value, Type? type = null) => Convert.ToDouble(value, CultureInfo.InvariantCulture);
    public void Update(IPort port, object value) => ((NumericPort)port).Value = ToNativeType(value);
}
