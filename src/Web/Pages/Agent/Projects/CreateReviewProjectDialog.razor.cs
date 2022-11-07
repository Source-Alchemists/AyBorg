using Autodroid.SDK.Data.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Autodroid.Web.Pages.Agent.Projects;

public partial class CreateReviewProjectDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public ProjectMetaDto Project { get; set; } = null!;

    private ProjectMetaDto TmpProject = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        TmpProject = Project with { };
    }

    private void OnCloseClicked()
    {
        MudDialog.Cancel();
    }

    private void OnCreateClicked()
    {
        MudDialog.Close(DialogResult.Ok(TmpProject));
    }
}