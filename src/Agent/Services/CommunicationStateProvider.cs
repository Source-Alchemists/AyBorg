using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Runtime;

namespace AyBorg.Agent.Services;

public sealed record CommunicationStateProvider : ICommunicationStateProvider
{
    /// <summary>
    /// Gets a value indicating whether the result communication is enabled.
    /// </summary>
    public bool IsResultCommunicationEnabled { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the web ui communication is enabled.
    /// </summary>
    public bool IsWebUiCommunicationEnabled { get; private set; }

    /// <summary>
    /// Updates the communication state.
    /// </summary>
    /// <param name="project">The project.</param>
    public void Update(Project project)
    {
        IsResultCommunicationEnabled = project.Meta.State == ProjectState.Ready;
        IsWebUiCommunicationEnabled = project.Meta.State != ProjectState.Ready;

        if (project.Settings.IsResultCommunicationForced)
        {
            IsResultCommunicationEnabled = true;
        }

        if (project.Settings.IsWebUiCommunicationForced)
        {
            IsWebUiCommunicationEnabled = true;
        }
    }
}
