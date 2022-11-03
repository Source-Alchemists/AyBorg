using Atomy.SDK.Data.DTOs;
using Atomy.SDK.Communication.MQTT;
using Microsoft.AspNetCore.SignalR.Client;

namespace Atomy.Web.Services.Agent;

public interface IFlowService
{
    /// <summary>
    /// Creates the hub connection.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns>The hub connection.</returns>
    HubConnection CreateHubConnection(string baseUrl);

    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns>The steps.</returns>
    Task<IEnumerable<StepDto>> GetStepsAsync(string baseUrl);

    /// <summary>
    /// Gets the links.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns>The links.</returns>
    Task<IEnumerable<LinkDto>> GetLinksAsync(string baseUrl);

    /// <summary>
    /// Adds the step.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    Task<StepDto> AddStepAsync(string baseUrl, Guid stepId, int x, int y);

    /// <summary>
    /// Removes the step.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <returns></returns>
    Task<bool> TryRemoveStepAsync(string baseUrl, Guid stepId);

    /// <summary>
    /// Moves the step.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    Task<bool> TryMoveStepAsync(string baseUrl, Guid stepId, int x, int y);

    /// <summary>
    /// Add link between ports.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="sourcePortId">The source port identifier.</param>
    /// <param name="targetPortId">The target port identifier.</param>
    /// <returns></returns>
    Task<bool> TryAddLinkAsync(string baseUrl, Guid sourcePortId, Guid targetPortId);

    /// <summary>
    /// Removes the link.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="linkId">The link identifier.</param>
    /// <returns></returns>
    Task<bool> TryRemoveLinkAsync(string baseUrl, Guid linkId);

    /// <summary>
    /// Gets the port.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="portId">The port identifier.</param>
    /// <returns></returns>
    Task<PortDto> GetPortAsync(string baseUrl, Guid portId);

    /// <summary>
    /// Gets the port for the given iteration.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="portId">The port identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <returns></returns>
    Task<PortDto> GetPortAsync(string baseUrl, Guid portId, Guid iterationId);

    /// <summary>
    /// Try to set the port value.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    Task<bool> TrySetPortValueAsync(string baseUrl, PortDto port);

    /// <summary>
    /// Gets the step execution time asynchronous.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <returns></returns>
    Task<long> GetStepExecutionTimeAsync(string baseUrl, Guid stepId, Guid iterationId);
}