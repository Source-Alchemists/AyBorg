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
    /// Adds the step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns>Added step.</returns>
    ValueTask<Step> AddStepAsync(Step step);

    /// <summary>
    /// Removes the step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <returns></returns>
    ValueTask<bool> TryRemoveStepAsync(Step step);

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
    /// <param name="sourcePort">The source port.</param>
    /// <param name="targetPort">The target port.</param>
    /// <returns></returns>
    ValueTask<Guid?> AddLinkAsync(Port sourcePort, Port targetPort);

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
    /// Gets the step.
    /// </summary>
    /// <param name="stepId">The step id.</param>
    /// <param name="iterationId">The iteration id.</param>
    /// <param name="updatePorts">Whether to update the ports.</param>
    /// <param name="skipOutputPorts">Whether to skip output ports.</param>
    /// <returns>The step.</returns>
    ValueTask<Step> GetStepAsync(Guid stepId, Guid? iterationId = null, bool updatePorts = true, bool skipOutputPorts = true);

    /// <summary>
    /// Gets the link.
    /// </summary>
    /// <param name="linkId">The link identifier.</param>
    /// <returns>The link.</returns>
    ValueTask<Link> GetLinkAsync(Guid linkId);

    /// <summary>
    /// Try to set the port value.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    ValueTask<bool> TrySetPortValueAsync(Port port);
}
