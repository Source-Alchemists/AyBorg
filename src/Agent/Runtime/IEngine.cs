using Atomy.SDK.System.Runtime;

namespace Atomy.Agent.Runtime;

public interface IEngine : IDisposable
{
    /// <summary>
    /// Called when the iteration is finished.
    /// </summary>
    event EventHandler<IterationFinishedEventArgs>? IterationFinished;

    /// <summary>
    /// Called when the engine state is changed.
    /// </summary>
    event EventHandler<EngineState>? StateChanged;

    /// <summary>
    /// Gets the identifier.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the state.
    /// </summary>
    EngineState State { get; }

    /// <summary>
    /// Gets the execution type.
    /// </summary>
    EngineExecutionType ExecutionType { get; }

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <returns></returns>
    Task<bool> TryStartAsync();

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns></returns>
    Task<bool> TryStopAsync();

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns></returns>
    Task<bool> TryAbortAsync();
}