namespace Autodroid.Agent.Runtime;

internal sealed class IterationFinishedEventArgs : EventArgs
{
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
    /// <param name="iterationId">The iteration identifier.</param>
    /// <param name="success">if set to <c>true</c> [success].</param>
    public IterationFinishedEventArgs(Guid iterationId, bool success)
    {
        IterationId = iterationId;
        Success = success;
    }
}