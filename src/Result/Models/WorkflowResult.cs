using AyBorg.SDK.Common.Models;

namespace AyBorg.Result;

public sealed record WorkflowResult
{
    public string ServiceUniqueName { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public string IterationId { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime StopTime { get; init; }
    public int ElapsedMs { get; init; }
    public bool Success { get; init; }
    public IEnumerable<Port> Ports { get; init; } = Array.Empty<Port>();
}
