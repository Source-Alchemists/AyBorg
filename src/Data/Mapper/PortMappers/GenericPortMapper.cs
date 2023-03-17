using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public static class GenericPortMapper
{
    public static Port ToRecord(IPort port)
    {
        return new Port
        {
            Id = port.Id,
            Name = port.Name,
            Direction = port.Direction,
            Brand = port.Brand,
            IsConnected = port.IsConnected
        };
    }
}
