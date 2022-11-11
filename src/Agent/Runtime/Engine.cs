using Autodroid.SDK.Common;
using Autodroid.SDK.Common.Ports;
using Autodroid.SDK.Communication.MQTT;
using Autodroid.SDK.Projects;
using Autodroid.SDK.System.Runtime;

namespace Autodroid.Agent.Runtime;

/// <summary>
/// Represents the engine.
/// </summary>
/// <remarks>To keep things simple, the engine is always running once and is not beeing reused. 
/// Each single/continuous run will create a new engine, with a new ID starting from idle state.</remarks>
internal sealed class Engine : IEngine
{
    private readonly ILogger<Engine> _logger;
    private readonly ILogger<PathExecuter> _pathExecuterLogger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMqttClientProvider _mqttClientProvider;
    private readonly Project _project;
    private readonly CancellationTokenSource _abortTokenSource = new();
    private readonly CancellationTokenSource _stopTokenSource = new();
    private Task? _executionTask;
    private bool _isDisposed = false;
    private Guid _iterationId = Guid.Empty;

    /// <summary>
    /// Called when the iteration is finished.
    /// </summary>
    public event EventHandler<IterationFinishedEventArgs>? IterationFinished;

    /// <summary>
    /// Called when the engine state is changed.
    /// </summary>
    public event EventHandler<EngineState>? StateChanged;

    /// <summary>
    /// Gets the identifier.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the state.
    /// </summary>
    public EngineState State { get; private set; } = EngineState.Idle;

    /// <summary>
    /// Gets the execution type.
    /// </summary>
    public EngineExecutionType ExecutionType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Engine"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="mqttClientProvider">The MQTT client provider.</param>
    /// <param name="project">The project.</param>
    /// <param name="executionType">Type of the execution.</param>
    public Engine(ILogger<Engine> logger, ILoggerFactory loggerFactory, IMqttClientProvider mqttClientProvider, Project project, EngineExecutionType executionType)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _mqttClientProvider = mqttClientProvider;
        _project = project;
        _pathExecuterLogger = _loggerFactory.CreateLogger<PathExecuter>();
        ExecutionType = executionType;

        foreach (var step in _project.Steps)
        {
            step.Completed += StepCompleted;
        }

        _logger.LogTrace("Engine [{Id}] with execution type [{executionType}] created.", Id, executionType);
    }

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<bool> TryStartAsync()
    {
        if (State != EngineState.Idle)
        {
            _logger.LogWarning($"Engine is not in idle state. Cannot start.");
            return false;
        }

        _logger.LogTrace("Engine [{Id}] starting.", Id);
        State = EngineState.Starting;
        StateChanged?.Invoke(this, State);

        var pathfinder = new Pathfinder();
        var pathItems = await pathfinder.CreatePathAsync(_project.Steps, _project.Links);
        _logger.LogTrace("Engine [{Id}] path created.", Id);

        _executionTask = Task.Factory.StartNew(async () => await ExecutePathAsync(pathItems, _stopTokenSource.Token, _abortTokenSource.Token), TaskCreationOptions.LongRunning);
        _logger.LogTrace("Engine [{Id}] execution started.", Id);

        State = EngineState.Running;
        StateChanged?.Invoke(this, State);
        return true;
    }

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<bool> TryStopAsync()
    {
        if (ExecutionType == EngineExecutionType.SingleRun)
        {
            _logger.LogWarning($"Engine is a single run engine. Cannot stop.");
            return false;
        }
        if (State != EngineState.Running)
        {
            _logger.LogWarning($"Engine is not in running state. Cannot stop.");
            return false;
        }

        _logger.LogTrace("Engine [{Id}] stopping.", Id);
        State = EngineState.Stopping;
        StateChanged?.Invoke(this, State);
        _stopTokenSource.Cancel();
        return await ValueTask.FromResult(true);
    }

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<bool> TryAbortAsync()
    {
        if (State != EngineState.Running && State != EngineState.Stopping)
        {
            _logger.LogWarning($"Engine is not in running or stopping state. Cannot abort.");
            return false;
        }

        _logger.LogTrace("Engine [{Id}] aborting.", Id);
        State = EngineState.Aborting;
        StateChanged?.Invoke(this, State);
        _abortTokenSource.Cancel();
        return await ValueTask.FromResult(true);
    }

    /// <summary>
    /// Disposes the engine.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (isDisposing && !_isDisposed)
        {
            if (_executionTask != null && !_executionTask.IsCompleted)
            {
                _logger.LogWarning("Engine [{Id}] is still running while disposing. Aborting.", Id);
                _abortTokenSource.Cancel();
                _executionTask.Wait();
                _executionTask.Dispose();
            }

            _abortTokenSource.Dispose();
            _stopTokenSource.Dispose();
            foreach (var step in _project.Steps)
            {
                step.Completed -= StepCompleted;
            }

            _isDisposed = true;
        }
    }

    private async ValueTask ExecutePathAsync(IEnumerable<PathItem> pathItems, CancellationToken stopToken, CancellationToken abortToken)
    {
        var executers = new HashSet<PathExecuter>();
        var executingTasks = new List<Task<bool>>();

        while (!stopToken.IsCancellationRequested && !abortToken.IsCancellationRequested)
        {
            _iterationId = Guid.NewGuid();
            _logger.LogTrace("Engine [{Id}] iteration [{_iterationId}] started.", Id, _iterationId);

            // Create a new executer for each path item.
            foreach (var pathItem in pathItems)
            {
                // If the same step is used multiple times in the path, we only need to create one executer for it.
                if (executers.Any(e => e.PathItem.Step.Id.Equals(pathItem.Step.Id)))
                {
                    continue;
                }

                executers.Add(new PathExecuter(_pathExecuterLogger, pathItem, _iterationId, abortToken));
            }

            // Wait till all path items are done with there work.
            // Done could be completed successfully or failed.
            while (!executers.All(e => e.Done))
            {
                foreach (var executer in executers.Where(e => e.State == PathExecutionState.Ready && e.State != PathExecutionState.Running))
                {
                    executingTasks.Add(executer.TryExecuteAsync());
                }

                // ToDo: Potentiall performance issue. Task delay could have a jitter of few milliseconds.
                await Task.Delay(1);
            }

            await WaitAndClearExecutors(executers, executingTasks);

            // All steps are executed and the iteration is finished.
            IterationFinished?.Invoke(this, new IterationFinishedEventArgs(_iterationId, executers.All(e => e.State == PathExecutionState.Completed)));

            _logger.LogTrace("Engine [{Id}] iteration [{_iterationId}] finished.", Id, _iterationId);

            // If the execution type is single run, stop the engine.
            if (ExecutionType == EngineExecutionType.SingleRun)
            {
                break;
            }
        }

        // If the engine is stopped, set the state to stopped.
        // If the engine is aborted, set the state to aborted.
        // Else set the state to idle.
        if (_abortTokenSource.IsCancellationRequested)
        {
            State = EngineState.Aborted;
            _logger.LogTrace("Engine [{Id}] aborted.", Id);
        }
        else if (_stopTokenSource.IsCancellationRequested)
        {
            State = EngineState.Stopped;
            _logger.LogTrace("Engine [{Id}] stopped.", Id);
        }
        else
        {
            State = EngineState.Finished;
            _logger.LogTrace("Engine [{Id}] single run finished.", Id);
        }
        StateChanged?.Invoke(this, State);
    }

    private async void StepCompleted(object? sender, bool success)
    {
        if (sender is not IStepProxy stepProxy) return;

        _logger.LogTrace("Step [{stepProxy.Name}] [{stepProxy.Id}] completed with result [{success}].", stepProxy.Name, stepProxy.Id, success);
        if (!success)
        {
            _logger.LogWarning("Step [{stepProxy.Name}] failed.", stepProxy.Name);
        }

        await SendStepInfoAsync(stepProxy);
    }

    private async ValueTask SendStepInfoAsync(IStepProxy stepProxy)
    {
        await SendStepInputPortsAsync(stepProxy);

        var baseTopic = $"Autodroid/agents/{_mqttClientProvider.ServiceUniqueName}/engine/steps/{stepProxy.Id}/";
        await _mqttClientProvider.PublishAsync($"{baseTopic}executionTimeMs", stepProxy.ExecutionTimeMs.ToString(), new MqttPublishOptions());
    }

    private async ValueTask SendStepInputPortsAsync(IStepProxy stepProxy)
    {
        var baseTopic = $"Autodroid/agents/{_mqttClientProvider.ServiceUniqueName}/engine/steps/{stepProxy.Id}/ports/";

        var inputPorts = stepProxy.StepBody.Ports.Where(p => p.Direction == PortDirection.Input);
        var token = CancellationToken.None;
        await Parallel.ForEachAsync(inputPorts, async (port, token) =>
        {
            await _mqttClientProvider.PublishAsync($"{baseTopic}{port.Id}", port, new MqttPublishOptions { Resize = true });
        });
    }

    private static async ValueTask WaitAndClearExecutors(HashSet<PathExecuter> executers, List<Task<bool>> executingTasks)
    {
        await Task.WhenAll(executingTasks);
        foreach (var t in executingTasks)
        {
            t.Dispose();
        }

        // Clear the executer list.
        foreach (var executer in executers)
        {
            executer.Dispose();
        }
        executers.Clear();
        executingTasks.ForEach(t => t.Dispose());
        executingTasks.Clear();
    }
}