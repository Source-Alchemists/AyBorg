using AyBorg.SDK.System.Runtime;

namespace AyBorg.Web.Shared.Models;

public record AgentServiceEntry
{
    public string Name { get; set; }
    public string EditorLink { get; }
    public string ProjectsLink { get; }
    public string DevicesLink { get; }
    public string ActiveProjectName { get; init; } = string.Empty;
    public EngineMeta Status { get; init; } = new EngineMeta();

    public AgentServiceEntry(ServiceInfoEntry serviceInfoEntry)
    {
        Name = serviceInfoEntry.Name.Replace("AyBorg.", string.Empty);
        EditorLink = $"agents/editor/{serviceInfoEntry.Id}";
        ProjectsLink = $"agents/projects/{serviceInfoEntry.Id}";
        DevicesLink = $"agents/devices/{serviceInfoEntry.Id}";
    }
}
