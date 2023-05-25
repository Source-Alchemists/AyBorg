using AyBorg.SDK.System.Runtime;


namespace AyBorg.Web.Services.Agent;

public interface IRuntimeService
{
    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The status.</returns>
    ValueTask<EngineMeta> GetStatusAsync(string serviceUniqueName);

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns>The status</returns>
    ValueTask<EngineMeta> StartRunAsync(string serviceUniqueName, EngineExecutionType executionType);

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The status</returns>
    ValueTask<EngineMeta> StopRunAsync(string serviceUniqueName);

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The status</returns>
    ValueTask<EngineMeta> AbortRunAsync(string serviceUniqueName);
}
