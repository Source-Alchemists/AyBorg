namespace AyBorg.Web.Shared.Models.Agent;

public sealed record ProjectSettings
{
    public bool IsForceResultCommunicationEnabled { get; set; }

    public ProjectSettings(Ayborg.Gateway.Agent.V1.ProjectSettingsDto projectSettings)
    {
        IsForceResultCommunicationEnabled = projectSettings.IsForceResultCommunicationEnabled;
    }
}
