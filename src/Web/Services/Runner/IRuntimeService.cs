using Microsoft.AspNetCore.SignalR.Client;
using Atomy.SDK.Runtime;


namespace Atomy.Web.Services.Agent;

public interface IRuntimeService
{
    /// <summary>
    /// Creates the hub connection.
    /// </summary>
    /// <returns>The hub connection.</returns>
    HubConnection CreateHubConnection();

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <returns>The status.</returns>
    Task<EngineMeta> GetStatusAsync();

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns>The status.</returns>
    Task<EngineMeta> GetStatusAsync(string baseUrl);

    /// <summary>
    /// Starts the engine.
    /// </summary>
    /// <param name="executionType">Type of the execution.</param>
    /// <returns>The status</returns>
    Task<EngineMeta> StartRunAsync(EngineExecutionType executionType);

    /// <summary>
    /// Stops the engine.
    /// </summary>
    /// <returns>The status</returns>
    Task<EngineMeta> StopRunAsync();

    /// <summary>
    /// Aborts the engine.
    /// </summary>
    /// <returns>The status</returns>
    Task<EngineMeta> AbortRunAsync();
}