namespace AyBorg.Web.Shared.Models.Cognitive;

public record ProjectMeta
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Creator { get; init; } = string.Empty;
    public DateTime Created { get; init; }
    public ProjectType Type { get; init; } = ProjectType.ObjectDetection;
    public IEnumerable<string> Tags { get; init; } = Array.Empty<string>();
    public IEnumerable<ClassLabel> Classes { get; init; } = Array.Empty<ClassLabel>();
}
