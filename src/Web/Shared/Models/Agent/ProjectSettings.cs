namespace AyBorg.Web.Shared.Models.Agent;

public sealed record ProjectSettings
{
    public bool IsForceResultCommunicationEnabled { get; set; }
    public bool IsForceWebUiCommunicationEnabled { get; set; }

    public ProjectSettings(Ayborg.Gateway.Agent.V1.ProjectSettings projectSettings)
    {
        IsForceResultCommunicationEnabled = projectSettings.IsForceResultCommunicationEnabled;
        IsForceWebUiCommunicationEnabled = projectSettings.IsForceWebUiCommunicationEnabled;
    }
}
