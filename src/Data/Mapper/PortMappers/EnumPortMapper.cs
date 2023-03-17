using System.Text.Json;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public sealed class EnumPortMapper : IPortMapper<Enum>
{
    public object ToNativeObject(object value, Type? type = null) => ToNativeType(value);
    public Enum ToNativeType(object value, Type? type = null)
    {
        EnumRecord record;
        if (value is Enum enumValue)
        {
            record = new EnumRecord
            {
                Name = enumValue.ToString(),
                Names = Enum.GetNames(enumValue.GetType())
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

        return (Enum)Enum.Parse(type!, record.Name);
    }
    public void Update(IPort port, object value)
    {
        var typedPort = (EnumPort)port;
        typedPort.Value = ToNativeType(value, typedPort.Value.GetType());
    }
}
