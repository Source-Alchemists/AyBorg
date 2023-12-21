using System.Collections.Immutable;

namespace AyBorg.Web.Shared.Models.Net;

public record ProjectMeta
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Creator { get; init; } = string.Empty;
    public DateTime Created { get; init; }
    public ProjectType Type { get; init; } = ProjectType.ObjectDetection;
    public ImmutableList<Tag> Tags { get; init; } = ImmutableList<Tag>.Empty;
}