using AyBorg.Diagrams.Core.Models;
using AyBorg.SDK.Common.Models;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

public class FlowNode : NodeModel
{
    /// <summary>
    /// Gets the step.
    /// </summary>
    public Step Step { get; private set; }

    /// <summary>
    /// Called when a step is updated.
    /// </summary>
    public Action StepChanged { get; set; } = null!;

    /// <summary>
    /// Called when a step is deleted.
    /// </summary>
    public Action OnDelete { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowNode"/> class.
    /// </summary>
    /// <param name="flowService">The flow service.</param>
    /// <param name="step">The step.</param>
    /// <param name="locked">Whether the node is locked.</param>
    public FlowNode(Step step, bool locked = false) : base(step.Id.ToString(), new Diagrams.Core.Geometry.Point(step.X, step.Y))
    {
        Title = step.Name;
        Step = step;
        Locked = locked;

        if (step.Ports == null) return;
        foreach (Port port in step.Ports)
        {
            _ = AddPort(new FlowPort(this, port) { Locked = locked });
        }
    }

    /// <summary>
    /// Updates the step.
    /// </summary>
    /// <param name="newStep">The new step.</param>
    public void Update(Step newStep)
    {
        Step.ExecutionTimeMs = newStep.ExecutionTimeMs;

        foreach (FlowPort targetFlowPort in Ports.Cast<FlowPort>())
        {
            Port sourcePort = newStep.Ports!.FirstOrDefault(p => p != null && p.Id.Equals(targetFlowPort.Port.Id))!;
            if (sourcePort == null)
            {
                continue;
            }
            targetFlowPort.Update(targetFlowPort.Port with { Value = sourcePort.Value });
        }

        StepChanged?.Invoke();
    }

    /// <summary>
    /// Deletes the step.
    /// </summary>
    public void Delete() => OnDelete?.Invoke();
}
