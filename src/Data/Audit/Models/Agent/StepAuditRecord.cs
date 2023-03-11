namespace AyBorg.Data.Audit.Models.Agent;

public record StepAuditRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public string AssemblyName { get; set; } = string.Empty;
    public string AssemblyVersion { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public List<PortAuditRecord> Ports { get; set; } = new();
}
