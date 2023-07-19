using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Agent;

#nullable disable

public record PortRecord
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    [Key]
    public Guid DbId { get; init; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the direction.
    /// </summary>
    public PortDirection Direction { get; init; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the brand.
    /// </summary>
    public PortBrand Brand { get; init; }

    /// <summary>
    /// Gets or sets the step record identifier.
    /// </summary>
    /// <remarks>Used by entity.</remarks>
    public Guid StepRecordId { get; init; }

    public Guid DeviceRecordId { get; init; }

    /// <summary>
    /// Gets or sets the step record.
    /// </summary>
    /// <remarks>Used by entity.</remarks>
    [JsonIgnore]
    public StepRecord StepRecord { get; init; }

    [JsonIgnore]
    public DeviceRecord DeviceRecord { get; init; }
}
