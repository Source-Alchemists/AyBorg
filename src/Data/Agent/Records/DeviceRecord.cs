using System.ComponentModel.DataAnnotations;

namespace AyBorg.Data.Agent;

#nullable disable

public sealed record DeviceRecord
{
    [Key]
    public Guid DbId { get; init; }

    public string Id { get; init; }

    public string Name { get; init; }

    public bool IsActive { get; set; }

    public PluginMetaInfoRecord MetaInfo { get; init; }

    /// <summary>
    /// Gets or sets the provider meta information.
    /// </summary>
    public PluginMetaInfoRecord ProviderMetaInfo { get; init; }

    public IEnumerable<DevicePortRecord> Ports { get; init; }
}
