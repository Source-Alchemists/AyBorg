namespace AyBorg.Agent.Runtime;

public sealed class IterationStartedEventArgs : EventArgs
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
    /// Initializes a new instance of the <see cref="IterationStartedEventArgs"/> class.
    /// </summary>
    /// <param name="engineId">The engine identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    public IterationStartedEventArgs(Guid engineId, Guid iterationId)
    {
        EngineId = engineId;
        IterationId = iterationId;
    }
}
