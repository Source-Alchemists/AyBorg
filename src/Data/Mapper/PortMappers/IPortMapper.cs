using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public interface IPortMapper
{
    object ToNativeObject(object value, Type? type = null);
    void Update(IPort port, object value);
}
