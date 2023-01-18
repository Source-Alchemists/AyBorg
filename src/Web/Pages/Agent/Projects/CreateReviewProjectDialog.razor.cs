using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Projects;

public partial class CreateReviewProjectDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public ProjectMeta Project { get; set; } = null!;

    private ProjectMeta _tmpProject = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _tmpProject = Project with {  Comment = string.Empty };
    }

    private void OnCloseClicked()
    {
        MudDialog.Cancel();
    }

    private void OnCreateClicked()
    {
        MudDialog.Close(DialogResult.Ok(new ProjectSaveInfo
        {
            State = SDK.Projects.ProjectState.Review,
            VersionName = _tmpProject.VersionName,
            Comment = _tmpProject.Comment
        }));
    }
}
