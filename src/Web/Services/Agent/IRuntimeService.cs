using AyBorg.SDK.System.Runtime;


namespace AyBorg.Web.Services.Agent;

public interface IRuntimeService
{
    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <returns>The status.</returns>
    ValueTask<EngineMeta> GetStatusAsync();

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The status.</returns>
    ValueTask<EngineMeta> GetStatusAsync(string serviceUniqueName);

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns>The status</returns>
    ValueTask<EngineMeta> StartRunAsync(EngineExecutionType executionType);

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>The status</returns>
    ValueTask<EngineMeta> StopRunAsync();

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>The status</returns>
    ValueTask<EngineMeta> AbortRunAsync();
}
