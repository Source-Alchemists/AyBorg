using Autodroid.Agent.Runtime;
using Autodroid.SDK.Communication.MQTT;
using Autodroid.SDK.Projects;
using Autodroid.SDK.System.Runtime;

namespace Autodroid.Agent.Services;

internal sealed class EngineFactory : IEngineFactory
{
    private readonly ILogger<EngineFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMqttClientProvider _mqttClientProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EngineFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="mqttClientProvider">The MQTT client provider.</param>
    public EngineFactory(ILogger<EngineFactory> logger, ILoggerFactory loggerFactory, IMqttClientProvider mqttClientProvider) {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _mqttClientProvider = mqttClientProvider;
    }

    /// <summary>
    /// Creates the engine.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns></returns>
    public IEngine CreateEngine(Project project, EngineExecutionType executionType) {
        _logger.LogTrace("Creating engine with execution type [{executionType}].", executionType);
        var engineLogger = _loggerFactory.CreateLogger<Engine>();
        return new Engine(engineLogger, _loggerFactory, _mqttClientProvider, project, executionType);
    }
}