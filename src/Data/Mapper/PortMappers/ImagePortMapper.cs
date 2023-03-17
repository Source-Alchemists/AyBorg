using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Data.Mapper;

public sealed class ImagePortMapper : IPortMapper<Image>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public Image ToNativeValue(object value, Type? type = null) => null!;
    public void Update(IPort port, object value) => ((ImagePort)port).Value = ToNativeValue(value);
    public Port ToModel(IPort port)
    {
        var typedPort = (ImagePort)port;

        CacheImage cacheImage;
        if (typedPort.Value != null)
        {
            cacheImage = new CacheImage
            {
                Meta = new ImageMeta
                {
                    Width = typedPort.Value.Width,
                    Height = typedPort.Value.Height,
                    PixelFormat = typedPort.Value.PixelFormat,
                },
                OriginalImage = new Image(typedPort.Value)
            };
        }
        else
        {
            cacheImage = new CacheImage();
        }

        return new Port
        {
            Id = port.Id,
            Name = port.Name,
            Direction = port.Direction,
            Brand = port.Brand,
            IsConnected = port.IsConnected,
            IsLinkConvertable = typedPort.IsLinkConvertable,
            Value = cacheImage
        };
    }
}
