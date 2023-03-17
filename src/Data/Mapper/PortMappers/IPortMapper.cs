using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public interface IPortMapper
{
    object ToNativeValueObject(object value, Type? type = null);
    void Update(IPort port, object value);
    Port ToRecord(IPort port);
}
