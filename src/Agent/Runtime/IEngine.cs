using Autodroid.SDK.System.Runtime;

namespace Autodroid.Agent.Runtime;

internal interface IEngine : IDisposable
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
    ValueTask<bool> TryStartAsync();

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns></returns>
    ValueTask<bool> TryStopAsync();

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns></returns>
    ValueTask<bool> TryAbortAsync();
}