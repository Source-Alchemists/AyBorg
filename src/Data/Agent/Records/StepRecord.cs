using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AyBorg.Data.Agent;

public record StepRecord
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
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the meta information.
    /// </summary>
    public PluginMetaInfoRecord MetaInfo { get; init; } = new PluginMetaInfoRecord();

    /// <summary>
    /// Gets or sets the x.
    /// </summary>
    public int X { get; init; }

    /// <summary>
    /// Gets or sets the y.
    /// </summary>
    public int Y { get; init; }

    /// <summary>
    /// Gets or sets the ports.
    /// </summary>
    public List<StepPortRecord> Ports { get; init; } = new List<StepPortRecord>();

    /// <summary>
    /// Gets or sets the project record identifier.
    /// </summary>
    /// <remarks>Used by entity for navigation.</remarks>
    public Guid ProjectRecordId { get; init; }

    /// <summary>
    /// Gets or sets the project record.
    /// </summary>
    /// <remarks>Used by entity for navigation.</remarks>
    [JsonIgnore]
    public ProjectRecord ProjectRecord { get; init; } = new ProjectRecord();
}
