using Autodroid.Agent.Runtime;
using Autodroid.SDK.Projects;
using Autodroid.SDK.System.Runtime;

namespace Autodroid.Agent.Services;

internal interface IEngineFactory
{
    /// <summary>
    /// Creates the engine.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns></returns>
    IEngine CreateEngine(Project project, EngineExecutionType executionType);
}