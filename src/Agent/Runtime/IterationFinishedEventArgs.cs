namespace AyBorg.Agent.Runtime;

public sealed class IterationFinishedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the engine identifier.
    /// </summary>
    public Guid EngineId { get; }

    /// <summary>
    /// Gets the iteration identifier.
    /// </summary>
    public Guid IterationId { get; }

    /// <summary>
    /// Gets the result.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IterationFinishedEventArgs"/> class.
    /// </summary>
    /// <param name="engineId">The engine identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="success">if set to <c>true</c> [success].</param>
    public IterationFinishedEventArgs(Guid engineId, Guid iterationId, bool success)
    {
        EngineId = engineId;
        IterationId = iterationId;
        Success = success;
    }
}
