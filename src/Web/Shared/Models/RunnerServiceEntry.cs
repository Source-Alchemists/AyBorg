using Atomy.SDK.Data.DTOs;
using Atomy.SDK.System.Runtime;

namespace Atomy.Web.Shared.Models;

public record AgentServiceEntry
{
    public string Name { get; set; }
    public string EditorLink { get; }
    public string ProjectsLink { get; }
    public string ActiveProjectName { get; init; } = string.Empty;
    public EngineMeta Status { get; init; } = new EngineMeta();

    public AgentServiceEntry(ServiceRegistryEntryDto dto)
    {
        Name = dto.Name.Replace("Atomy.", string.Empty);
        EditorLink = $"agents/editor/{dto.Id}";
        ProjectsLink = $"agents/projects/{dto.Id}";
    }
}