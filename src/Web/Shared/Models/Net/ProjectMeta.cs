using System.Collections.Immutable;
using AyBorg.Web.Shared.Modals.Net;

namespace AyBorg.Web.Shared.Models.Net;

internal record ProjectMeta
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ProjectType Type { get; init; } = ProjectType.ObjectDetection;
    public ImmutableList<Tag> Tags { get; init; } = ImmutableList<Tag>.Empty;
}