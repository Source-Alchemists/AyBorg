using Atomy.Agent.Runtime;
using Atomy.SDK;
using Atomy.SDK.Runtime;

namespace Atomy.Agent.Services;

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