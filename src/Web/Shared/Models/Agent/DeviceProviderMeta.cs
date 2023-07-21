namespace AyBorg.Web.Shared.Models.Agent;

public sealed record DeviceProviderMeta
{
    public string Name { get; init; } = string.Empty;
    public string Prefix { get; init; } = string.Empty;
    public bool CanAdd { get; init; } = false;
    public IReadOnlyCollection<DeviceMeta> Devices { get; init; } = Array.Empty<DeviceMeta>();
}
