using AyBorg.SDK.Common.Models;

namespace AyBorg.Web.Shared.Models.Agent;

public record DeviceMeta
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; } = false;
    public bool IsConnected { get; init; } = false;
    public IReadOnlyCollection<string> Categories { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<Port> Ports { get; init; } = Array.Empty<Port>();
}
