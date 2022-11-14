using AyBorg.SDK.Data.DTOs;
using AyBorg.SDK.Projects;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Projects;

public partial class CreateReviewProjectDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public ProjectMetaDto Project { get; set; } = null!;

    private ProjectMetaDto _tmpProject = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _tmpProject = Project with { };
    }

    private void OnCloseClicked()
    {
        MudDialog.Cancel();
    }

    private void OnCreateClicked()
    {
        MudDialog.Close(DialogResult.Ok(new ProjectStateChangeDto
        {
            State = ProjectState.Review,
            VersionName = _tmpProject.VersionName,
            Comment = _tmpProject.Comment
        }));
    }
}
