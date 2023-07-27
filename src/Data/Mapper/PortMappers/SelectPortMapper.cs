using System.Text.Json;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public sealed class SelectPortMapper : IPortMapper<SelectPort.ValueContainer>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public SelectPort.ValueContainer ToNativeValue(object value, Type? type = null)
    {
        if (value is SelectPort.ValueContainer container)
        {
            return container;
        }

        return JsonSerializer.Deserialize<SelectPort.ValueContainer>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
    public void Update(IPort port, object value) => ((SelectPort)port).Value = ToNativeValue(value);
    public Port ToModel(IPort port)
    {
        var typedPort = (SelectPort)port;
        return new Port
        {
            Id = port.Id,
            Name = port.Name,
            Direction = port.Direction,
            Brand = port.Brand,
            IsConnected = port.IsConnected,
            IsLinkConvertable = typedPort.IsLinkConvertable,
            Value = typedPort.Value
        };
    }
}
