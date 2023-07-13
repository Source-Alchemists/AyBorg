namespace AyBorg.Web.Shared.Models.Agent;

internal record DeviceMeta
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Categories { get; init; } = Array.Empty<string>();
    public bool IsActive { get; init; } = false;
    public bool IsConnected { get; init; } = false;
}
