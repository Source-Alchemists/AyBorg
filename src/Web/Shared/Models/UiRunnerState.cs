namespace AyBorg.Web.Shared.Models;

public record UiAgentState
{
    public string Name { get; init; } = string.Empty;
    public string UniqueName { get; init; } = string.Empty;
    public string EditorLink { get; set; } = string.Empty;
    public string ProjectsLink { get; set; } = string.Empty;

    public UiAgentState()
    {
    }

    public UiAgentState(ServiceInfoEntry serviceInfoEntry)
    {
        Name = serviceInfoEntry.Name;
        UniqueName = serviceInfoEntry.UniqueName;
        var agentEntry = new AgentServiceEntry(serviceInfoEntry);
        EditorLink = agentEntry.EditorLink;
        ProjectsLink = agentEntry.ProjectsLink;
    }
}
