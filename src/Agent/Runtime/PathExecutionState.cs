namespace Atomy.Agent.Runtime;

public enum PathExecutionState
{
    /// <summary>
    /// The path item is ready to be executed.
    /// </summary>
    Ready,

    /// <summary>
    /// The path item is waiting for execution.
    /// </summary>
    Waiting,

    /// <summary>
    /// The path item is currently executing.
    /// </summary>
    Running,

    /// <summary>
    /// The path item has completed execution.
    /// </summary>
    Completed,

    /// <summary>
    /// The path item has failed execution.
    /// </summary>
    Failed
}