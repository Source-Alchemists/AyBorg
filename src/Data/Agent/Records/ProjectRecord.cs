using System.ComponentModel.DataAnnotations;

namespace AyBorg.Data.Agent;

public record ProjectRecord
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    [Key]
    public Guid DbId { get; set; }

    /// <summary>
    /// Gets or sets the project meta record.
    /// </summary>
    /// <remarks>Used for navigation.</remarks>
    public ProjectMetaRecord Meta { get; set; } = new ProjectMetaRecord();

    /// <summary>
    /// Gets or sets the project settings record.
    /// </summary>
    public ProjectSettingsRecord Settings { get; set; } = new ProjectSettingsRecord();

    /// <summary>
    /// Gets or sets the steps.
    /// </summary>
    /// <value>
    /// The steps.
    /// </value>
    public List<StepRecord> Steps { get; set; } = new List<StepRecord>();

    /// <summary>
    /// Gets or sets the links.
    /// </summary>
    public List<LinkRecord> Links { get; set; } = new List<LinkRecord>();
}
