using System.Globalization;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public class BooleanPortMapper : IPortMapper<bool>
{
    public object ToNativeObject(object value, Type? type = null) => ToNativeType(value);
    public bool ToNativeType(object value, Type? type = null)  => Convert.ToBoolean(value, CultureInfo.InvariantCulture);
    public void Update(IPort port, object value) => ((BooleanPort)port).Value = ToNativeType(value);
}
