using AyBorg.Agent.Runtime;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Runtime;

namespace AyBorg.Agent.Services;

public interface IEngineFactory
{
    /// <summary>
    /// Creates the engine.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns></returns>
    IEngine CreateEngine(Project project, EngineExecutionType executionType);
}
