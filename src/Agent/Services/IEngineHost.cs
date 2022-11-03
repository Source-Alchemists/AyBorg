using Atomy.SDK.Projects;
using Atomy.SDK.System.Runtime;

namespace Atomy.Agent.Services;

public interface IEngineHost : IDisposable
{
    /// <summary>
    /// Gets the active project.
    /// </summary>
    Project? ActiveProject { get; }

    /// <summary>
    /// Tries to activate the specified project.
    /// </summary>
    /// <param name="project">The project.</param>
    Task<bool> TryActivateProjectAsync(Project project);

    /// <summary>
    /// Tries to deactivate the project.
    /// </summary>
    /// <returns></returns>
    Task<bool> TryDeactivateProjectAsync();

    /// <summary>
    /// Gets the engine status asynchronous.
    /// </summary>
    /// <returns></returns>
    Task<EngineMeta> GetEngineStatusAsync();

    /// <summary>
    /// Start the engine.
    /// </summary>
    /// <param name="executionType">The execution type.</param>
    /// <returns>Engine meta informations.</returns>
    Task<EngineMeta> StartRunAsync(EngineExecutionType executionType);

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>Engine meta informations.</returns>
    Task<EngineMeta> StopRunAsync();

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>Engine meta informations.</returns>
    Task<EngineMeta> AbortRunAsync();
}