using System.Collections.Immutable;
using System.Text.Json;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public class StringCollectionPortMapper : IPortMapper<ImmutableList<string>>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public ImmutableList<string> ToNativeValue(object value, Type? type = null)
    {
        List<string> record;
        if (value is ImmutableList<string> collection)
        {
            record = collection.ToList();
        }
        else
        {
            record = JsonSerializer.Deserialize<List<string>>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        // Check for null strings as we only allow empty strings, not null.
        if (record.Any(r => r == null))
        {
            var newCollection = new List<string>();
            foreach (string s in record)
            {
                if (s == null)
                {
                    newCollection.Add(string.Empty);
                }
                else
                {
                    newCollection.Add(s);
                }
            }
            record = newCollection;
        }

        return record.ToImmutableList();
    }

    public void Update(IPort port, object value) => ((StringCollectionPort)port).Value = ToNativeValue(value);
    public Port ToModel(IPort port)
    {
        var typedPort = (StringCollectionPort)port;
        return new Port
        {
            Id = port.Id,
            Name = port.Name,
            Direction = port.Direction,
            Brand = port.Brand,
            IsConnected = port.IsConnected,
            IsLinkConvertable = typedPort.IsLinkConvertable,
            Value = JsonSerializer.Serialize(typedPort.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        };
    }
}
