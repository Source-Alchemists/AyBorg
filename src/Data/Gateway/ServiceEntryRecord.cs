using System.ComponentModel.DataAnnotations;

namespace AyBorg.Data.Gateway;

public record ServiceEntryRecord
{
    /// <summary>
    /// Gets or sets the unique service id.
    /// After registration, the id is used to identify the service.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The unique service name.
    /// </summary>
    public string UniqueName { get; set; } = string.Empty;

    /// <summary>
    /// The service type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address.
    /// </summary>
    public string Url { get; set; } = string.Empty;
}
