using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Data.Mapper;

public sealed class ImagePortMapper : IPortMapper<Image>
{
    public object ToNativeObject(object value, Type? type = null) => ToNativeObject(value);
    public Image ToNativeType(object value, Type? type = null) => null!;
    public void Update(IPort port, object value) => ((ImagePort)port).Value = ToNativeType(value);
}
