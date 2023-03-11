using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Audit.Models.Agent;

public record PortAuditRecord
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public PortBrand Brand { get; init; }
    public PortDirection Direction { get; init; }
}
