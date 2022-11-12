using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Agent.Services;

public interface IFlowService
{
    /// <summary>
    /// Get steps.
    /// </summary>
    /// <returns></returns>
    IEnumerable<IStepProxy> GetSteps();

    /// <summary>
    /// Gets the links.
    /// </summary>
    IEnumerable<PortLink> GetLinks();

    /// <summary>
    /// Adds the step.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>The new created step proxy.</returns>
    ValueTask<IStepProxy> AddStepAsync(Guid stepId, int x, int y);

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
    /// Link ports together.
    /// </summary>
    /// <param name="sourcePortId">The source port identifier.</param>
    /// <param name="targetPortId">The target port identifier.</param>
    /// <returns></returns>
    ValueTask<PortLink> LinkPortsAsync(Guid sourcePortId, Guid targetPortId);

    /// <summary>
    /// Unlink ports.
    /// </summary>
    /// <param name="linkId">The link identifier.</param>
    /// <returns></returns>
    ValueTask<bool> TryUnlinkPortsAsync(Guid linkId);

    /// <summary>
    /// Gets the port.
    /// </summary>
    /// <param name="portId">The port identifier.</param>
    /// <returns></returns>
    ValueTask<IPort> GetPortAsync(Guid portId);

    /// <summary>
    /// Updates the port value.
    /// </summary>
    /// <param name="portId">The port identifier.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    ValueTask<bool> TryUpdatePortValueAsync(Guid portId, object value);
}