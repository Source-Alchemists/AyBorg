using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Runtime;

internal sealed class PathExecuter : IDisposable
{
    private readonly ILogger<PathExecuter> _logger;
    private readonly CancellationToken _abortToken;
    private bool _isDisposed = false;

    /// <summary>
    /// Gets the target iteration identifier the executer is running for.
    /// </summary>
    public Guid TargetIterationId { get; private set; }

    /// <summary>
    /// Gets the state of the path executer.
    /// </summary>
    public PathExecutionState State { get; private set; } = PathExecutionState.Ready;

    /// <summary>
    /// Gets a value indicating whether the path executer done.
    /// </summary>
    public bool Done => State == PathExecutionState.Completed || State == PathExecutionState.Failed;

    /// <summary>
    /// Gets the path item.
    /// </summary>
    public PathItem PathItem { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PathExecuter"/> class.
    /// </summary>
    /// <param name="pathItem">The path item.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="abortToken">The abort token.</param>
    public PathExecuter(ILogger<PathExecuter> logger, PathItem pathItem, Guid iterationId, CancellationToken abortToken)
    {
        _logger = logger;
        PathItem = pathItem;
        _abortToken = abortToken;
        TargetIterationId = iterationId;

        foreach (IStepProxy pred in PathItem.Predecessors)
        {
            State = PathExecutionState.Waiting; // Waiting to be executed until all predecessors are completed.
            pred.Completed += PredecessorCompleted;
        }
    }

    /// <summary>
    /// Executes the path item.
    /// </summary>
    /// <returns>The execution result.</returns>
    public async Task<bool> TryExecuteAsync()
    {
        if (State != PathExecutionState.Ready) throw new InvalidOperationException("Path item is not ready to be executed.");

        State = PathExecutionState.Running;

        bool stepResult = false;
        try
        {
            stepResult = await PathItem.Step.TryRunAsync(TargetIterationId, _abortToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error occurred while executing step [{name}]", PathItem.Step.Name);
        }
        finally
        {
            State = stepResult ? PathExecutionState.Completed : PathExecutionState.Failed;

        }
        return stepResult;
    }

    /// <summary>
    /// Dispose the path executer.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void PredecessorCompleted(object? sender, bool successful)
    {
        if (!successful) State = PathExecutionState.Failed; // No need to execute the step if a predecessor failed.
        if (PathItem.Predecessors.All(p => p.IterationId.Equals(TargetIterationId)))
        {
            State = PathExecutionState.Ready;
        }
    }

    private void Dispose(bool isDisposing)
    {
        if (isDisposing && !_isDisposed)
        {
            foreach (IStepProxy pred in PathItem.Predecessors)
            {
                pred.Completed -= PredecessorCompleted;
            }

            _isDisposed = true;
        }
    }
}
