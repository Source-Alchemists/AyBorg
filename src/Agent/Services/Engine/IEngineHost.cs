using AyBorg.Agent.Runtime;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Runtime;

namespace AyBorg.Agent.Services;

public interface IEngineHost : IDisposable
{
    /// <summary>
    /// Occurs when [iteration started].
    /// </summary>
    event EventHandler<IterationStartedEventArgs> IterationStarted;

    /// <summary>
    /// Occurs when [iteration finished].
    /// </summary>
    event EventHandler<IterationFinishedEventArgs> IterationFinished;

    /// <summary>
    /// Gets the active project.
    /// </summary>
    Project? ActiveProject { get; }

    /// <summary>
    /// Tries to activate the specified project.
    /// </summary>
    /// <param name="project">The project.</param>
    ValueTask<bool> TryActivateProjectAsync(Project project);

    /// <summary>
    /// Tries to deactivate the project.
    /// </summary>
    /// <returns></returns>
    ValueTask<bool> TryDeactivateProjectAsync();

    /// <summary>
    /// Gets the engine status.
    /// </summary>
    /// <returns></returns>
    EngineMeta GetEngineStatus();

    /// <summary>
    /// Start the engine.
    /// </summary>
    /// <param name="executionType">The execution type.</param>
    /// <returns>Engine meta informations.</returns>
    ValueTask<EngineMeta> StartRunAsync(EngineExecutionType executionType);

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>Engine meta informations.</returns>
    ValueTask<EngineMeta> StopRunAsync();

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>Engine meta informations.</returns>
    ValueTask<EngineMeta> AbortRunAsync();
}
