using AyBorg.Web.Shared.Models.Cognitive;

namespace AyBorg.Web.Shared.Models;

public record UiCognitiveState
{
    public string ProjectId { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public AnnotationState Annotation { get; init; } = null!;

    public UiCognitiveState() {}
    public UiCognitiveState(ProjectMeta projectMeta)
    {
        ProjectId = projectMeta.Id;
        ProjectName = projectMeta.Name;
        Annotation = new AnnotationState(Array.Empty<string>(), 0);
    }

    public record AnnotationState (IEnumerable<string> SelectedImageNames, int SelectedImageIndex);
}
