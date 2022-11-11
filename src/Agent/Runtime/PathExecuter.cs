namespace Autodroid.Agent.Runtime;

internal sealed class PathExecuter : IDisposable
{
    private readonly ILogger<PathExecuter> _logger;
    private readonly PathItem _pathItem;
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
    public PathItem PathItem => _pathItem;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathExecuter"/> class.
    /// </summary>
    /// <param name="pathItem">The path item.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="abortToken">The abort token.</param>
    public PathExecuter(ILogger<PathExecuter> logger, PathItem pathItem, Guid iterationId, CancellationToken abortToken)
    {
        _logger = logger;
        _pathItem = pathItem;
        _abortToken = abortToken;
        TargetIterationId = iterationId;

        foreach (var pred in _pathItem.Predecessors)
        {
            State = PathExecutionState.Waiting; // Waiting to be executed until all predecessors are completed.
            pred.Completed += PredecessorCompleted;
        }
    }

    /// <summary>
    /// Executes the path item.
    /// </summary>
    /// <returns>The execution result.</returns>
    public Task<bool> TryExecuteAsync()
    {
        if (State != PathExecutionState.Ready) throw new InvalidOperationException("Path item is not ready to be executed.");

        State = PathExecutionState.Running;
        return Task.Run(async () =>
        {
            bool stepResult = false;
            try
            {
                stepResult = await _pathItem.Step.TryRunAsync(TargetIterationId, _abortToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred while executing step [{name}]", _pathItem.Step.Name);
            }
            finally
            {
                State = stepResult ? PathExecutionState.Completed : PathExecutionState.Failed;

            }
            return stepResult;
        });
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
        if (_pathItem.Predecessors.All(p => p.IterationId.Equals(TargetIterationId)))
        {
            State = PathExecutionState.Ready;
        }
    }

    private void Dispose(bool isDisposing)
    {
        if (isDisposing && !_isDisposed)
        {
            foreach (var pred in _pathItem.Predecessors)
            {
                pred.Completed -= PredecessorCompleted;
            }

            _isDisposed = true;
        }
    }
}