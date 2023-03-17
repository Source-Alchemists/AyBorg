using System.Collections.ObjectModel;
using System.Text.Json;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public sealed class NumericCollectionPortMapper : IPortMapper<ReadOnlyCollection<double>>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public ReadOnlyCollection<double> ToNativeValue(object value, Type? type = null)
    {
        if (value is ReadOnlyCollection<double> collection)
        {
            return new ReadOnlyCollection<double>(collection);
        }
        else
        {
            return new ReadOnlyCollection<double>(JsonSerializer.Deserialize<List<double>>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!);
        }
    }
    public void Update(IPort port, object value) => ((NumericCollectionPort)port).Value = ToNativeValue(value);
    public Port ToModel(IPort port)
    {
        var typedPort = (NumericCollectionPort)port;
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
