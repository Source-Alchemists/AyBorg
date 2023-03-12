using System.ComponentModel.DataAnnotations;

namespace AyBorg.Data.Agent;

public record PluginMetaInfoRecord
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    [Key]
    public Guid DbId { get; set; }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the assembly.
    /// </summary>
    /// <value>
    /// The name of the assembly.
    /// </value>
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assembly version.
    /// </summary>
    /// <value>
    /// The assembly version.
    /// </value>
    public string AssemblyVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the type.
    /// </summary>
    /// <value>
    /// The name of the type.
    /// </value>
    public string TypeName { get; set; } = string.Empty;
}
