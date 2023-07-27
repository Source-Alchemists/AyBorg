namespace AyBorg.Web.Shared.Models;

public record UiAgentState
{
    public string ServiceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string UniqueName { get; init; } = string.Empty;
    public string EditorLink { get; set; } = string.Empty;
    public string ProjectsLink { get; set; } = string.Empty;
    public string DevicesLink { get; set; } = string.Empty;

    public UiAgentState()
    {
    }

    public UiAgentState(ServiceInfoEntry serviceInfoEntry)
    {
        ServiceId = serviceInfoEntry.Id;
        Name = serviceInfoEntry.Name;
        UniqueName = serviceInfoEntry.UniqueName;
        var agentEntry = new AgentServiceEntry(serviceInfoEntry);
        EditorLink = agentEntry.EditorLink;
        ProjectsLink = agentEntry.ProjectsLink;
        DevicesLink = agentEntry.DevicesLink;
    }
}
