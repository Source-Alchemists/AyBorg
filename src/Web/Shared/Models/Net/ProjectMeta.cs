using System.Collections.Immutable;

namespace AyBorg.Web.Shared.Models.Net;

public record ProjectMeta
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Creator { get; init; } = string.Empty;
    public DateTime Created { get; init; }
    public ProjectType Type { get; init; } = ProjectType.ObjectDetection;
    public ImmutableList<string> Tags { get; init; } = ImmutableList<string>.Empty;
    public ImmutableList<Label> Labels { get; init; } = ImmutableList<Label>.Empty;
}