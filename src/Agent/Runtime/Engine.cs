using System.Runtime.CompilerServices;
using AyBorg.SDK.Common;
using AyBorg.SDK.Projects;
using AyBorg.SDK.System.Runtime;

namespace AyBorg.Agent.Runtime;

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
    private readonly Project _project;
    private readonly CancellationTokenSource _abortTokenSource = new();
    private readonly CancellationTokenSource _stopTokenSource = new();
    private Task? _executionTask;
    private bool _isDisposed = false;

    /// <summary>
    /// Called when the iteration is started.
    /// </summary>
    public event EventHandler<IterationStartedEventArgs>? IterationStarted;

    /// <summary>
    /// Called when the iteration is finished.
    /// </summary>
    public event EventHandler<IterationFinishedEventArgs>? IterationFinished;

    /// <summary>
    /// Called when the engine state is changed.
    /// </summary>
    public event EventHandler<EngineState>? StateChanged;

    /// <summary>
    /// Gets the meta information.
    /// </summary>
    public EngineMeta Meta { get; }

    /// <summary>
    /// Gets the execution type.
    /// </summary>
    public EngineExecutionType ExecutionType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Engine"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="project">The project.</param>
    /// <param name="executionType">Type of the execution.</param>
    public Engine(ILogger<Engine> logger, ILoggerFactory loggerFactory, Project project, EngineExecutionType executionType)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _project = project;
        _pathExecuterLogger = _loggerFactory.CreateLogger<PathExecuter>();
        ExecutionType = executionType;
        Meta = new EngineMeta
        {
            Id = Guid.NewGuid(),
            State = EngineState.Idle,
            ExecutionType = executionType
        };

        foreach (IStepProxy step in _project.Steps)
        {
            step.Completed += StepCompleted;
        }

        StateChanged += StateChangedCallback;

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(new EventId((int)EventLogType.Engine), "Engine [{Id}] with execution type [{executionType}] created.", Meta.Id, executionType);
        }
    }

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<bool> TryStartAsync()
    {
        if (Meta.State != EngineState.Idle)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), $"Engine is not in idle state. Cannot start.");
            return false;
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(new EventId((int)EventLogType.Engine), "Engine [{Id}] starting.", Meta.Id);
        }
        Meta.State = EngineState.Starting;
        StateChanged?.Invoke(this, Meta.State);

        var pathfinder = new Pathfinder();
        IEnumerable<PathItem> pathItems = await pathfinder.CreatePathAsync(_project.Steps, _project.Links);
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(new EventId((int)EventLogType.Engine), "Engine [{Id}] path created.", Meta.Id);
        }

        _executionTask = Task.Factory.StartNew(async () => await ExecutePathAsync(pathItems, _stopTokenSource.Token, _abortTokenSource.Token), TaskCreationOptions.LongRunning);
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(new EventId((int)EventLogType.Engine), "Engine [{Id}] execution started.", Meta.Id);
        }

        Meta.State = EngineState.Running;
        StateChanged?.Invoke(this, Meta.State);
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
            _logger.LogWarning(new EventId((int)EventLogType.Engine), $"Engine is a single run engine. Cannot stop.");
            return false;
        }
        if (Meta.State != EngineState.Running)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), $"Engine is not in running state. Cannot stop.");
            return false;
        }

        _logger.LogTrace("Engine [{Id}] stopping.", Meta.Id);
        Meta.State = EngineState.Stopping;
        StateChanged?.Invoke(this, Meta.State);
        _stopTokenSource.Cancel();
        return await ValueTask.FromResult(true);
    }

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns></returns>
    public async ValueTask<bool> TryAbortAsync()
    {
        if (Meta.State != EngineState.Running && Meta.State != EngineState.Stopping)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Engine), $"Engine is not in running or stopping state. Cannot abort.");
            return false;
        }

        _logger.LogTrace(new EventId((int)EventLogType.Engine), "Engine [{Id}] aborting.", Meta.Id);
        Meta.State = EngineState.Aborting;
        StateChanged?.Invoke(this, Meta.State);
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
                _logger.LogWarning(new EventId((int)EventLogType.Engine), "Engine [{Id}] is still running while disposing. Aborting.", Meta.Id);
                _abortTokenSource.Cancel();
                _executionTask.Wait();
                _executionTask.Dispose();
            }

            _abortTokenSource.Dispose();
            _stopTokenSource.Dispose();
            foreach (IStepProxy step in _project.Steps)
            {
                step.Completed -= StepCompleted;
            }

            StateChanged -= StateChangedCallback;
            _isDisposed = true;
        }
    }

    private async ValueTask ExecutePathAsync(IEnumerable<PathItem> pathItems, CancellationToken stopToken, CancellationToken abortToken)
    {
        var executers = new HashSet<PathExecuter>();
        var executingTasks = new List<Task<bool>>();

        while (!stopToken.IsCancellationRequested && !abortToken.IsCancellationRequested)
        {
            var iterationId = Guid.NewGuid();
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(new EventId((int)EventLogType.Engine), "Engine [{Id}] iteration [{_iterationId}] started.", Meta.Id, iterationId);
            }

            IterationStarted?.Invoke(this, new IterationStartedEventArgs(Meta.Id, iterationId));

            await StartExecuteAllPathItemsAsync(_pathExecuterLogger, iterationId, executers, pathItems, executingTasks, abortToken);
            await WaitAndClearExecutors(executers, executingTasks);

            // All steps are executed and the iteration is finished.
            IterationFinished?.Invoke(this, new IterationFinishedEventArgs(Meta.Id, iterationId, executers.All(e => e.State == PathExecutionState.Completed)));

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(new EventId((int)EventLogType.Engine), "Engine [{Id}] iteration [{_iterationId}] finished.", Meta.Id, iterationId);
            }

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
            Meta.State = EngineState.Aborted;
        }
        else if (_stopTokenSource.IsCancellationRequested)
        {
            Meta.State = EngineState.Stopped;
        }
        else
        {
            Meta.State = EngineState.Finished;
        }

        StateChanged?.Invoke(this, Meta.State);
    }

    private void StepCompleted(object? sender, bool success)
    {
        if (sender is not IStepProxy stepProxy) return;

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(new EventId((int)EventLogType.Plugin), "Step [{stepProxy.Name}] [{stepProxy.Id}] completed with result [{success}].", stepProxy.Name, stepProxy.Id, success);
        }

        if (!success)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Step [{stepProxy.Name}] failed.", stepProxy.Name);
        }
    }

    private void StateChangedCallback(object? sender, EngineState state)
    {
        if (state == EngineState.Stopped)
        {
            _logger.LogInformation(new EventId((int)EventLogType.Engine), "Engine stopped at {DateTime.UtcNow} (UTC).", DateTime.UtcNow);
        }
        else if (state == EngineState.Aborted)
        {
            _logger.LogInformation(new EventId((int)EventLogType.Engine), "Engine aborted at {DateTime.UtcNow} (UTC).", DateTime.UtcNow);
        }
        else if (state == EngineState.Finished)
        {
            _logger.LogInformation(new EventId((int)EventLogType.Engine), "Engine finished single run at {DateTime.UtcNow} (UTC).", DateTime.UtcNow);
        }
        else if (state == EngineState.Running)
        {
            _logger.LogInformation(new EventId((int)EventLogType.Engine), "Engine started at {DateTime.UtcNow} (UTC).", DateTime.UtcNow);
        }

        if (state == EngineState.Stopped || state == EngineState.Aborted || state == EngineState.Finished)
        {
            Meta.StoppedAt = DateTime.UtcNow;
            _logger.LogTrace($"Engine is done. Removing engine.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask StartExecuteAllPathItemsAsync(ILogger<PathExecuter> pathExecuterLogger, Guid iterationId,
                                                                HashSet<PathExecuter> executers, IEnumerable<PathItem> pathItems,
                                                                List<Task<bool>> executingTasks, CancellationToken abortToken)
    {
        // Create a new executer for each path item.
        foreach (PathItem pathItem in pathItems)
        {
            // If the same step is used multiple times in the path, we only need to create one executer for it.
            if (executers.Any(e => e.PathItem.Step.Id.Equals(pathItem.Step.Id)))
            {
                continue;
            }

            executers.Add(new PathExecuter(pathExecuterLogger, pathItem, iterationId, abortToken));
        }

        // Wait till all path items are done with there work.
        // Done could be completed successfully or failed.
        while (!executers.All(e => e.Done))
        {
            foreach (PathExecuter? executer in executers.Where(e => e.State == PathExecutionState.Ready && e.State != PathExecutionState.Running))
            {
                executingTasks.Add(executer.TryExecuteAsync());
            }

            // ToDo: Potentiall performance issue. Task delay could have a jitter of few milliseconds.
            await Task.Delay(1, CancellationToken.None);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask WaitAndClearExecutors(HashSet<PathExecuter> executers, List<Task<bool>> executingTasks)
    {
        await Task.WhenAll(executingTasks);
        foreach (Task<bool> t in executingTasks)
        {
            t.Dispose();
        }

        // Clear the executer list.
        foreach (PathExecuter executer in executers)
        {
            executer.Dispose();
        }
        executers.Clear();
        executingTasks.ForEach(t => t.Dispose());
        executingTasks.Clear();
    }
}
