using System.Text.Json;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public sealed class EnumPortMapper : IPortMapper<System.Enum>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public System.Enum ToNativeValue(object value, Type? type = null)
    {
        EnumRecord record;
        if (value is System.Enum enumValue)
        {
            record = new EnumRecord
            {
                Name = enumValue.ToString(),
                Names = System.Enum.GetNames(enumValue.GetType())
            };
        }
        else if (value is SDK.Common.Models.Enum enumBinding)
        {
            record = new EnumRecord
            {
                Name = enumBinding.Name!,
                Names = enumBinding.Names!
            };
        }
        else
        {
            record = JsonSerializer.Deserialize<EnumRecord>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        return (System.Enum)System.Enum.Parse(type!, record.Name);
    }
    public void Update(IPort port, object value)
    {
        var typedPort = (EnumPort)port;
        typedPort.Value = ToNativeValue(value, typedPort.Value.GetType());
    }

    public Port ToModel(IPort port)
    {
        var typedPort = (EnumPort)port;
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
