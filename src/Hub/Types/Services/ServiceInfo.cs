using Redis.OM.Modeling;

namespace AyBorg.Hub.Types.Services;

[Document(StorageType = StorageType.Json, Prefixes = ["ServiceInfo"])]
public record ServiceInfo
{
    [RedisIdField]
    [Indexed]
    public string Id { get; init; } = string.Empty;

    [Indexed]
    public string Name { get; init; } = string.Empty;

    [Indexed]
    public string UniqueName { get; init; } = string.Empty;

    [Indexed]
    public string Type { get; init; } = string.Empty;

    [Indexed]
    public string Version { get; init; } = string.Empty;
}
