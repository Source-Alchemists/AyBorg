using Ayborg.Gateway.V1;

namespace AyBorg.Web.Shared.Models;

public record ServiceInfoEntry
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string UniqueName { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;

    public ServiceInfoEntry(ServiceInfo serviceInfo)
    {
        Id = serviceInfo.Id;
        Name = serviceInfo.Name;
        UniqueName = serviceInfo.UniqueName;
        Type = serviceInfo.Type;
        Url = serviceInfo.Url;
        Version = serviceInfo.Version;
    }
}
