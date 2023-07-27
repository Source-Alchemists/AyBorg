using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public static class PortMapperFactory
{
    public static IPortMapper CreateMapper(IPort port)
    {
        return port.Brand switch
        {
            PortBrand.String => new StringPortMapper(),
            PortBrand.Folder => new StringPortMapper(),
            PortBrand.Numeric => new NumericPortMapper(),
            PortBrand.Boolean => new BooleanPortMapper(),
            PortBrand.Enum => new EnumPortMapper(),
            PortBrand.Select => new SelectPortMapper(),
            PortBrand.Rectangle => new RectanglePortMapper(),
            PortBrand.Image => new ImagePortMapper(),
            PortBrand.StringCollection => new StringCollectionPortMapper(),
            PortBrand.NumericCollection => new NumericCollectionPortMapper(),
            PortBrand.RectangleCollection => new RectangleCollectionPortMapper(),
            _ => throw new ArgumentException($"No mapper exists for port brand {port.Brand}."),
        };
    }
}
