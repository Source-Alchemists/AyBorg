using System.ComponentModel.DataAnnotations;

namespace AyBorg.Data.Agent;

#nullable disable

public sealed record DeviceRecord
{
    [Key]
    public Guid DbId { get; init; }

    public string Id { get; init; }

    public string Name { get; init; }

    public bool IsActive { get; init; }

    public PluginMetaInfoRecord MetaInfo { get; init; }

    public IEnumerable<PortRecord> Ports { get; init; }
}
