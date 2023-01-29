using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Audit.Models.Agent;

public record PortAuditRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public PortBrand Brand { get; set; }
}
