using AyBorg.SDK.Common.Models;

namespace AyBorg.Web.Services.Agent;

public interface IFlowService
{
    /// <summary>
    /// Gets the steps.
    /// </summary>
    /// <returns>The steps.</returns>
    ValueTask<IEnumerable<Step>> GetStepsAsync();

    /// <summary>
    /// Gets the links.
    /// </summary>
    /// <returns>The links.</returns>
    ValueTask<IEnumerable<Link>> GetLinksAsync();

    /// <summary>
    /// Adds the step asynchronous.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    ValueTask<Step> AddStepAsync(Guid stepId, int x, int y);

    /// <summary>
    /// Removes the step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <returns></returns>
    ValueTask<bool> TryRemoveStepAsync(Guid stepId);

    /// <summary>
    /// Moves the step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    ValueTask<bool> TryMoveStepAsync(Guid stepId, int x, int y);

    /// <summary>
    /// Add link between ports.
    /// </summary>
    /// <param name="sourcePortId">The source port identifier.</param>
    /// <param name="targetPortId">The target port identifier.</param>
    /// <returns></returns>
    ValueTask<bool> TryAddLinkAsync(Guid sourcePortId, Guid targetPortId);

    /// <summary>
    /// Removes the link.
    /// </summary>
    /// <param name="linkId">The link identifier.</param>
    /// <returns></returns>
    ValueTask<bool> TryRemoveLinkAsync(Guid linkId);

    /// <summary>
    /// Gets the port for the given iteration.
    /// </summary>
    /// <param name="portId">The port identifier.</param>
    /// <param name="iterationId">The iteration identifier.</param>
    /// <returns></returns>
    ValueTask<Port> GetPortAsync(Guid portId, Guid? iterationId = null);

    /// <summary>
    /// Try to set the port value.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    ValueTask<bool> TrySetPortValueAsync(Port port);
}
