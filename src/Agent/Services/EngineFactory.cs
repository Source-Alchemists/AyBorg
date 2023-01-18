using AyBorg.Agent.Runtime;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Runtime;

namespace AyBorg.Agent.Services;

internal sealed class EngineFactory : IEngineFactory
{
    private readonly ILogger<EngineFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="EngineFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public EngineFactory(ILogger<EngineFactory> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Creates the engine.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns></returns>
    public IEngine CreateEngine(Project project, EngineExecutionType executionType)
    {
        _logger.LogTrace("Creating engine with execution type [{executionType}].", executionType);
        ILogger<Engine> engineLogger = _loggerFactory.CreateLogger<Engine>();
        return new Engine(engineLogger, _loggerFactory, project, executionType);
    }
}
