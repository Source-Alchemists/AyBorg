using System.Text.Json;
using Ayborg.Gateway.Agent.V1;
using AyBorg.Agent.Runtime;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Configuration;
using AyBorg.SDK.System.Runtime;
using Grpc.Core;

namespace AyBorg.Agent.Services;

internal sealed class EngineHost : IEngineHost
{
    private readonly ILogger<EngineHost> _logger;
    private readonly IEngineFactory _engineFactory;
    private readonly IMqttClientProvider _mqttClientProvider;
    private readonly ICacheService _cacheService;
    private readonly CommunicationStateProvider _communicationStateProvider;
    private readonly IServiceConfiguration _serviceConfiguration;
    private readonly Notify.NotifyClient _notifyClient;
    private IEngine? _engine;
    private EngineMeta? _engineMeta;
    private bool _isDisposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="EngineHost"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="engineFactory">The engine factory.</param>
    /// <param name="mqttClientProvider">The MQTT client provider.</param>
    public EngineHost(ILogger<EngineHost> logger,
                        IEngineFactory engineFactory,
                        IMqttClientProvider mqttClientProvider,
                        ICacheService cacheService,
                        ICommunicationStateProvider communicationStateProvider,
                        IServiceConfiguration serviceConfiguration,
                        Notify.NotifyClient notifyClient)
    {
        _logger = logger;
        _engineFactory = engineFactory;
        _mqttClientProvider = mqttClientProvider;
        _cacheService = cacheService;
        _communicationStateProvider = (CommunicationStateProvider)communicationStateProvider;
        _serviceConfiguration = serviceConfiguration;
        _notifyClient = notifyClient;
    }

    /// <summary>
    /// Gets the active project.
    /// </summary>
    public Project? ActiveProject { get; private set; }

    /// <summary>
    /// Tries to activate the specified project.
    /// </summary>
    /// <param name="project">The project.</param>
    public async ValueTask<bool> TryActivateProjectAsync(Project project)
    {
        ActiveProject = project;
        return await ValueTask.FromResult(true);
    }

    /// <summary>
    /// Tries to deactivate the project.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<bool> TryDeactivateProjectAsync()
    {
        if (ActiveProject is null)
        {
            _logger.LogTrace("No active project to deactivate.");
            return true;
        }

        if (_engine != null)
        {
            _engine.Dispose();
            _engine = null;
        }

        foreach (SDK.Common.IStepProxy step in ActiveProject.Steps)
        {
            step.Dispose();
        }

        ActiveProject = null;
        return await ValueTask.FromResult(true);
    }

    /// <summary>
    /// Gets the engine status asynchronous.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<EngineMeta> GetEngineStatusAsync()
    {
        if (_engine == null)
        {
            _logger.LogTrace("No active engine.");
            return new EngineMeta();
        }

        if (_engineMeta == null)
        {
            _logger.LogWarning("Engine meta is null.");
            return new EngineMeta();
        }

        _engineMeta.State = _engine.State;

        _logger.LogTrace("Engine status: {_engine.State}, {_engine.ExecutionType}", _engine.State, _engine.ExecutionType);
        return await ValueTask.FromResult(_engineMeta);
    }

    /// <summary>
    /// Start the engine.
    /// </summary>
    /// <param name="executionType">The execution type.</param>
    /// <returns>Engine meta informations.</returns>
    public async ValueTask<EngineMeta> StartRunAsync(EngineExecutionType executionType)
    {
        if (ActiveProject == null)
        {
            _logger.LogWarning("No active project.");
            return null!;
        }

        if (_engine != null
            && (_engine.State == EngineState.Running
                || _engine.State == EngineState.Stopping
                || _engine.State == EngineState.Aborting))
        {
            _logger.LogWarning("Engine is already running.");
            return null!;
        }

        // Dispose previous engine.
        DisposeEngine();

        _communicationStateProvider.Update(ActiveProject);

        _engine = _engineFactory.CreateEngine(ActiveProject, executionType);
        _engineMeta = new EngineMeta
        {
            Id = _engine.Id,
            State = EngineState.Idle,
            ExecutionType = executionType
        };
        _engine.StateChanged += EngineStateChanged;
        _engine.IterationFinished += EngineIterationFinished;
        bool startResult = await _engine.TryStartAsync();
        if (!startResult)
        {
            _logger.LogWarning("Engine start failed.");
            return null!;
        }
        _engineMeta.State = EngineState.Starting;
        return _engineMeta;
    }

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>Engine meta informations.</returns>
    public async ValueTask<EngineMeta> StopRunAsync()
    {
        if (_engine == null)
        {
            _logger.LogWarning("No active engine.");
            return null!;
        }

        if (_engine.State != EngineState.Running)
        {
            _logger.LogWarning("Engine is not running.");
            return null!;
        }

        if (_engineMeta == null)
        {
            _logger.LogWarning("Engine meta is null.");
            return null!;
        }

        bool stopResult = await _engine.TryStopAsync();
        if (!stopResult)
        {
            _logger.LogWarning("Engine stop failed.");
            return null!;
        }

        _engineMeta.State = EngineState.Stopping;
        return _engineMeta;
    }

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>Engine meta informations.</returns>
    public async ValueTask<EngineMeta> AbortRunAsync()
    {
        if (_engine == null)
        {
            _logger.LogWarning("No active engine.");
            return null!;
        }

        if (_engine.State == EngineState.Aborting)
        {
            _logger.LogWarning("Engine is already aborting.");
            return null!;
        }

        if (_engineMeta == null)
        {
            _logger.LogWarning("Engine meta is null.");
            return null!;
        }

        bool abortResult = await _engine.TryAbortAsync();
        if (!abortResult)
        {
            _logger.LogWarning("Engine abort failed.");
            return null!;
        }

        _engineMeta.State = EngineState.Aborting;
        return _engineMeta;
    }

    /// <summary>
    /// Disposes the engine host.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (isDisposing && !_isDisposed)
        {
            DisposeEngine();
            _isDisposed = true;
        }
    }

    private async void EngineStateChanged(object? sender, EngineState state)
    {
        if (_engineMeta == null)
        {
            _logger.LogWarning("Engine meta is null.");
            return;
        }

        _engineMeta.State = state;

        _logger.LogTrace("Engine state changed to '{state}'.", state);

        if (state == EngineState.Stopped)
        {
            _logger.LogInformation("Engine stopped at {DateTime.UtcNow} (UTC).", DateTime.UtcNow);
        }
        else if (state == EngineState.Aborted)
        {
            _logger.LogInformation("Engine aborted at {DateTime.UtcNow} (UTC).", DateTime.UtcNow);
        }
        else if (state == EngineState.Finished)
        {
            _logger.LogInformation("Engine finished single run at {DateTime.UtcNow} (UTC).", DateTime.UtcNow);
        }
        else if (state == EngineState.Running)
        {
            _logger.LogInformation("Engine started at {DateTime.UtcNow} (UTC).", DateTime.UtcNow);
        }

        if (state == EngineState.Stopped || state == EngineState.Aborted || state == EngineState.Finished)
        {
            _engineMeta.StoppedAt = DateTime.UtcNow;
            _logger.LogTrace($"Engine is done. Removing engine.");
        }

        await _mqttClientProvider.PublishAsync($"AyBorg/agents/{_mqttClientProvider.ServiceUniqueName}/engine/status", JsonSerializer.Serialize(_engineMeta), new MqttPublishOptions());
    }

    private void DisposeEngine()
    {
        if (_engine == null) return;
        _engine.StateChanged -= EngineStateChanged;
        _engine.IterationFinished -= EngineIterationFinished;
        _engine.Dispose();
        _engine = null;
    }

    private async void EngineIterationFinished(object? sender, IterationFinishedEventArgs e)
    {
        if (ActiveProject == null)
        {
            _logger.LogWarning("No active project.");
            return;
        }

        await _cacheService.CreateCacheAsync(e.IterationId, ActiveProject);
        try
        {
            await _notifyClient.EngineIterationFinishedAsync(new EngineIterationFinishedArgsDto
            {
                AgentUniqueName = _serviceConfiguration.UniqueName,
                IterationId = e.IterationId.ToString()
            });
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Failed to notify engine iteration finished.");
        }
    }
}
