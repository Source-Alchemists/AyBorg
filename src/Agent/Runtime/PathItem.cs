using Atomy.SDK.Common;

namespace Atomy.Agent.Runtime;

internal sealed record PathItem 
{
    /// <summary>
    // Gets the identifier of the path item.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the source step.
    /// </summary>
    public IStepProxy Step { get; }

    /// <summary>
    /// Gets the predecessor steps.
    /// </summary>
    public IList<IStepProxy> Predecessors { get; } = new List<IStepProxy>();

    /// <summary>
    /// Gets the successor steps.
    /// </summary>
    public IList<IStepProxy> Successors { get; } = new List<IStepProxy>();

    /// <summary>
    /// Gets a value indicating whether the path item is a start step.
    /// </summary>
    public bool IsStart { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether the path item is a end step.
    /// </summary>
    public bool IsEnd { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether the path item is a merge step.
    /// </summary>
    public bool IsMerge { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether the path item is a fork step.
    /// </summary>
    public bool IsFork { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathItem"/> class.
    /// </summary>
    /// <param name="current">The current step.</param>
    public PathItem(IStepProxy current)
    {
        Id = current.Id;
        Step = current;
    }
}