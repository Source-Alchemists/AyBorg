using System.Collections.Immutable;
using AyBorg.Web.Shared.Models.Net;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Net.Projects;

public partial class Projects : ComponentBase
{
    [Inject] IDialogService DialogService { get; init; } = null!;

    private bool _isLoading = true;
    private ImmutableList<ProjectMeta> _projects = ImmutableList<ProjectMeta>.Empty;
    
    private async Task OnNewProjectClicked()
    {
        IDialogReference dialogReference = DialogService.Show<NewProjectDialog>("Create Project", new DialogOptions {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });

        DialogResult result = await dialogReference.Result;
        if(!result.Canceled)
        {
            
        }
    }
}