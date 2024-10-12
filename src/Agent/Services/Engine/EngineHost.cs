/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using AyBorg.Agent.Runtime;
using AyBorg.Communication;
using AyBorg.Runtime;
using AyBorg.Runtime.Projects;
using AyBorg.Types;
using Grpc.Core;

namespace AyBorg.Agent.Services;

internal sealed class EngineHost : IEngineHost
{
    private readonly ILogger<EngineHost> _logger;
    private readonly IEngineFactory _engineFactory;
    private readonly ICacheService _cacheService;
    private readonly CommunicationStateProvider _communicationStateProvider;
    private readonly INotifyService _notifyService;
    private IEngine? _engine;
    private bool _isDisposed = false;

    /// <summary>
    /// Occurs when [iteration started].
    /// </summary>
    public event EventHandler<IterationStartedEventArgs>? IterationStarted;

    /// <summary>
    /// Occurs when [iteration finished].
    /// </summary>
    public event EventHandler<IterationFinishedEventArgs>? IterationFinished;

    /// <summary>
    /// Gets the active project.
    /// </summary>
    public Project? ActiveProject { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EngineHost"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="engineFactory">The engine factory.</param>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="communicationStateProvider">The communication state provider.</param>
    /// <param name="notifyService">The notify service.</param>
    public EngineHost(ILogger<EngineHost> logger,
                        IEngineFactory engineFactory,
                        ICacheService cacheService,
                        ICommunicationStateProvider communicationStateProvider,
                        INotifyService notifyService)
    {
        _logger = logger;
        _engineFactory = engineFactory;
        _cacheService = cacheService;
        _communicationStateProvider = (CommunicationStateProvider)communicationStateProvider;
        _notifyService = notifyService;
    }

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

        foreach (IStepProxy step in ActiveProject.Steps)
        {
            step.Dispose();
        }

        ActiveProject = null;
        return await ValueTask.FromResult(true);
    }

    /// <summary>
    /// Gets the engine status.
    /// </summary>
    /// <returns></returns>
    public EngineMeta GetEngineStatus()
    {
        if (_engine == null)
        {
            _logger.LogTrace(new EventId((int)EventLogType.Engine), "No active engine.");
            return new EngineMeta();
        }

        return _engine.Meta;
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
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project.");
            return new EngineMeta();
        }

        if (_engine != null
            && (_engine.Meta.State == EngineState.Running
                || _engine.Meta.State == EngineState.Stopping
                || _engine.Meta.State == EngineState.Aborting))
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), "Engine is already running.");
            return _engine.Meta;
        }

        // Dispose previous engine.
        DisposeEngine();

        _communicationStateProvider.Update(ActiveProject);

        _engine = _engineFactory.CreateEngine(ActiveProject, executionType);
        _engine.StateChanged += EngineStateChanged;
        _engine.IterationStarted += OnEngineIterationStarted;
        _engine.IterationFinished += OnEngineIterationFinished;
        bool startResult = await _engine.TryStartAsync();
        if (!startResult)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), "Engine start failed.");
        }

        return _engine.Meta;
    }

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>Engine meta informations.</returns>
    public async ValueTask<EngineMeta> StopRunAsync()
    {
        if (_engine == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), "No active engine.");
            return new EngineMeta();
        }

        if (_engine.Meta.State != EngineState.Running)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), "Engine is not running. ({State})", _engine.Meta.State);
            return _engine.Meta;
        }

        bool stopResult = await _engine.TryStopAsync();
        if (!stopResult)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), "Engine stop failed.");
        }

        return _engine.Meta;
    }

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>Engine meta informations.</returns>
    public async ValueTask<EngineMeta> AbortRunAsync()
    {
        if (_engine == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), "No active engine.");
            return new EngineMeta();
        }

        if (_engine.Meta.State == EngineState.Aborting)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), "Engine is already aborting.");
            return _engine.Meta;
        }

        bool abortResult = await _engine.TryAbortAsync();
        if (!abortResult)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), "Engine abort failed.");
        }

        return _engine.Meta;
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
        if (_engine == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), "No engine");
            return;
        }

        await _notifyService.SendEngineStateAsync(_engine.Meta);
    }

    private void DisposeEngine()
    {
        if (_engine == null) return;
        _engine.StateChanged -= EngineStateChanged;
        _engine.IterationStarted -= OnEngineIterationStarted;
        _engine.IterationFinished -= OnEngineIterationFinished;
        _engine.Dispose();
        _engine = null;
    }

    private void OnEngineIterationStarted(object? sender, IterationStartedEventArgs e)
    {
        IterationStarted?.Invoke(this, e);
    }

    private async void OnEngineIterationFinished(object? sender, IterationFinishedEventArgs e)
    {
        if (ActiveProject == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No active project.");
            return;
        }

        _cacheService.CreateCache(e.IterationId, ActiveProject);
        IterationFinished?.Invoke(this, e);

        try
        {
            await _notifyService.SendIterationFinishedAsync(e.IterationId);
        }
        catch (RpcException ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Engine), ex, "Failed to notify engine iteration finished.");
        }
    }
}
