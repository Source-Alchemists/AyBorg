using System.Text.Json.Serialization;
using AyBorg.SDK.Data.DTOs;

namespace AyBorg.Web.Shared.Models;

public record UiAgentState
{
    public string Name { get; init; } = string.Empty;
    public string UniqueName { get; init; } = string.Empty;

    [JsonIgnore]
    public string BaseUrl { get; set; } = string.Empty;
    public string EditorLink { get; set; } = string.Empty;
    public string ProjectsLink { get; set; } = string.Empty;

    public UiAgentState()
    {
    }

    public UiAgentState(RegistryEntryDto entry)
    {
        Name = entry.Name;
        UniqueName = entry.UniqueName;
        BaseUrl = entry.Url;
        var agentEntry = new AgentServiceEntry(entry);
        EditorLink = agentEntry.EditorLink;
        ProjectsLink = agentEntry.ProjectsLink;
    }
}