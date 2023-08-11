using System.Collections.Concurrent;
using System.Collections.Immutable;
using AyBorg.SDK.Common.Result;

namespace AyBorg.Agent.Result;

public record WorkflowResult
{
    public Guid IterationId { get; init; }
    public DateTime StartTime { get; set; }
    public DateTime StopTime { get; set; }
    public BlockingCollection<PortResult> PortResults { get; } = new BlockingCollection<PortResult>();
}
