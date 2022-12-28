using AyBorg.SDK.Common.Models;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

public class FlowNode : NodeModel
{
    public Step Step { get; private set; }

    /// <summary>
    /// Called when a step is updated.
    /// </summary>
    public Action StepChanged { get; set; } = null!;
    public Action OnDelete { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowNode"/> class.
    /// </summary>
    /// <param name="flowService">The flow service.</param>
    /// <param name="step">The step.</param>
    /// <param name="locked">Whether the node is locked.</param>
    public FlowNode(Step step, bool locked = false) : base(new Point(step.X, step.Y))
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

    public void Update(Step newStep)
    {
        Step.ExecutionTimeMs = newStep.ExecutionTimeMs;
        Position = new Point(newStep.X, newStep.Y);
        foreach (FlowPort targetFlowPort in Ports.Cast<FlowPort>())
        {
            Port sourcePort = newStep.Ports!.FirstOrDefault(p => p.Id.Equals(targetFlowPort.Port.Id))!;
            if (sourcePort == null)
            {
                continue;
            }

            targetFlowPort.Port.Value = sourcePort.Value;
            StepChanged?.Invoke();
        }
    }

    public void Delete()
    {
        OnDelete?.Invoke();
    }
}
