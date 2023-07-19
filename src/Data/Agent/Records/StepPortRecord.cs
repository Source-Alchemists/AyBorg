using System.Text.Json.Serialization;

namespace AyBorg.Data.Agent;

#nullable disable

public sealed record StepPortRecord : PortRecord
{
    /// <summary>
    /// Gets or sets the step record identifier.
    /// </summary>
    /// <remarks>Used by entity.</remarks>
    public Guid StepRecordId { get; init; }

    /// <summary>
    /// Gets or sets the step record.
    /// </summary>
    /// <remarks>Used by entity.</remarks>
    [JsonIgnore]
    public StepRecord StepRecord { get; init; }
}
