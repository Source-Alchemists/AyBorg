using System.Text.Json.Serialization;

namespace AyBorg.Data.Agent;

#nullable disable

public sealed record DevicePortRecord : PortRecord
{
    /// <summary>
    /// Gets or sets the device record identifier.
    /// </summary>
    /// <remarks>Used by entity.</remarks>
    public Guid DeviceRecordId { get; init; }

    /// <summary>
    /// Gets or sets the device record.
    /// </summary>
    /// <remarks>Used by entity.</remarks>
    [JsonIgnore]
    public DeviceRecord DeviceRecord { get; init; }
}
