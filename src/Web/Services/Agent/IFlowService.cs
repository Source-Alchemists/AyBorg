using AyBorg.SDK.Data.Bindings;

namespace AyBorg.Web.Services.Agent;

public interface IFlowService
{
    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The steps.</returns>
    ValueTask<IEnumerable<Step>> GetStepsAsync(string serviceUniqueName);

    /// <summary>
    /// Gets the links.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <returns>The links.</returns>
    ValueTask<IEnumerable<Link>> GetLinksAsync(string serviceUniqueName);

    /// <summary>
    /// Adds the step asynchronous.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    ValueTask<Step> AddStepAsync(string serviceUniqueName, Guid stepId, int x, int y);

    /// <summary>
    /// Removes the step.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <returns></returns>
    ValueTask<bool> TryRemoveStepAsync(string serviceUniqueName, Guid stepId);

    /// <summary>
    /// Moves the step.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    ValueTask<bool> TryMoveStepAsync(string serviceUniqueName, Guid stepId, int x, int y);

    /// <summary>
    /// Add link between ports.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="sourcePortId">The source port identifier.</param>
    /// <param name="targetPortId">The target port identifier.</param>
    /// <returns></returns>
    ValueTask<bool> TryAddLinkAsync(string serviceUniqueName, Guid sourcePortId, Guid targetPortId);

    /// <summary>
    /// Removes the link.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="linkId">The link identifier.</param>
    /// <returns></returns>
    ValueTask<bool> TryRemoveLinkAsync(string serviceUniqueName, Guid linkId);

    /// <summary>
    /// Gets the port for the given iteration.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="portId">The port identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <returns></returns>
    ValueTask<Port> GetPortAsync(string serviceUniqueName, Guid portId, Guid? iterationId = null);

    /// <summary>
    /// Try to set the port value.
    /// </summary>
    /// <param name="serviceUniqueName">The service unique name.</param>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    ValueTask<bool> TrySetPortValueAsync(string serviceUniqueName, Port port);
}
