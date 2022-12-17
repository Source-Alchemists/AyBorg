using Microsoft.AspNetCore.SignalR.Client;
using AyBorg.SDK.System.Runtime;


namespace AyBorg.Web.Services.Agent;

public interface IRuntimeService
{
    // /// <summary>
    // /// Creates the hub connection.
    // /// </summary>
    // /// <returns>The hub connection.</returns>
    // HubConnection CreateHubConnection();

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <returns>The status.</returns>
    ValueTask<EngineMeta> GetStatusAsync();

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
