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
    /// Updates the communication state.
    /// </summary>
    /// <param name="project">The project.</param>
    public void Update(Project project)
    {
        IsResultCommunicationEnabled = project.Meta.State == ProjectState.Ready;

        if (project.Settings.IsForceResultCommunicationEnabled)
        {
            IsResultCommunicationEnabled = !IsResultCommunicationEnabled;
        }
    }
}
