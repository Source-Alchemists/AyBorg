using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AyBorg.Data.Agent;

public record LinkRecord {

    /// <summary>
    /// Gets or sets the database identifier.
    [Key]
    public Guid DbId { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the source identifier.
    /// </summary>
    public Guid SourceId { get; set; }

    /// <summary>
    /// Gets or sets the target identifier.
    /// </summary>
    public Guid TargetId { get; set; }

    /// <summary>
    /// Gets or sets the project record identifier.
    /// </summary>
    /// <remarks>Used by entity for navigation.</remarks>
    public Guid ProjectRecordId { get; set; }

    /// <summary>
    /// Gets or sets the project record.
    /// </summary>
    /// <remarks>Used by entity for navigation.</remarks>
    [JsonIgnore]
    public ProjectRecord ProjectRecord { get; set; } = new ProjectRecord();
}
