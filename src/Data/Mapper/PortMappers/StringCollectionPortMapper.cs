using System.Collections.ObjectModel;
using System.Text.Json;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public class StringCollectionPortMapper : IPortMapper<ReadOnlyCollection<string>>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public ReadOnlyCollection<string> ToNativeValue(object value, Type? type = null)
    {
        List<string> record;
        if (value is ReadOnlyCollection<string> collection)
        {
            record = new ReadOnlyCollection<string>(collection).ToList();
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

        return new ReadOnlyCollection<string>(record);
    }

    public void Update(IPort port, object value) => ((StringCollectionPort)port).Value = ToNativeValue(value);
    public Port ToRecord(IPort port)
    {
        var typedPort = (StringCollectionPort)port;
        Port record = GenericPortMapper.ToRecord(typedPort);
        record.IsLinkConvertable = typedPort.IsLinkConvertable;
        record.Value = JsonSerializer.Serialize(typedPort.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return record;
    }
}
