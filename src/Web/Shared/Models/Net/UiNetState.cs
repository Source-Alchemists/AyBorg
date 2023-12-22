using AyBorg.Web.Shared.Models.Net;

namespace AyBorg.Web.Shared.Models;

public record UiNetState
{
    public string ProjectId { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;

    public UiNetState() {}
    public UiNetState(ProjectMeta projectMeta)
    {
        ProjectId = projectMeta.Id;
        ProjectName = projectMeta.Name;
    }
}