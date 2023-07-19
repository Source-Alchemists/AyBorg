using System.ComponentModel.DataAnnotations;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Agent;

#nullable disable

public abstract record PortRecord
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
}
