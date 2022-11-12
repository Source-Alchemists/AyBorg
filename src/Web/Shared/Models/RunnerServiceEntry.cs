using AyBorg.SDK.Data.DTOs;
using AyBorg.SDK.System.Runtime;

namespace AyBorg.Web.Shared.Models;

public record AgentServiceEntry
{
    public string Name { get; set; }
    public string EditorLink { get; }
    public string ProjectsLink { get; }
    public string ActiveProjectName { get; init; } = string.Empty;
    public EngineMeta Status { get; init; } = new EngineMeta();

    public AgentServiceEntry(RegistryEntryDto dto)
    {
        Name = dto.Name.Replace("AyBorg.", string.Empty);
        EditorLink = $"agents/editor/{dto.Id}";
        ProjectsLink = $"agents/projects/{dto.Id}";
    }
}