using System.Collections.Immutable;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Net;
using AyBorg.Web.Shared.Modals;
using AyBorg.Web.Shared.Models.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace AyBorg.Web.Pages.Net.Projects;

public partial class Projects : ComponentBase
{
    [CascadingParameter] Task<AuthenticationState> AuthenticationState { get; set; } = null!;
    [Inject] IDialogService DialogService { get; init; } = null!;
    [Inject] IProjectManagerService ProjectManagerService { get; init; } = null!;
    [Inject] IStateService StateService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    [Inject] NavigationManager NavigationManager { get; init; } = null!;

    private bool _isLoading = true;
    private string _username = string.Empty;
    private ImmutableList<ProjectMeta> _projects = ImmutableList<ProjectMeta>.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (AuthenticationState is not null)
        {
            AuthenticationState authState = await AuthenticationState;
            System.Security.Claims.ClaimsPrincipal user = authState?.User!;

            if (user?.Identity is not null)
            {
                _username = user.Identity.Name!;
            }
        }

        await UpdateProjectList();
    }

    private async ValueTask UpdateProjectList()
    {
        _isLoading = true;
        try
        {
            IEnumerable<ProjectMeta> projects = await ProjectManagerService.GetMetasAsync();
            _projects = _projects.Clear();
            _projects = _projects.AddRange(projects);
        }
        catch
        {
            // Already catched and logged.
            Snackbar.Add("Failed to load projects", Severity.Warning);
        }

        _isLoading = false;

    }

    private async void OnNewProjectClicked()
    {
        IDialogReference dialogReference = DialogService.Show<NewProjectDialog>("Create Project", new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });

        DialogResult result = await dialogReference.Result;
        if (!result.Canceled)
        {
            await UpdateProjectList();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnDeleteProjectClicked(ProjectMeta projectMeta)
    {
        IDialogReference dialogReference = DialogService.Show<ConfirmDialog>("Delete Project",
        new DialogParameters
        {
            {"NeedPassword", true},
            {"ContentText", $"Are you sure you want to delete the project '{projectMeta.Name}'? All associated data will be deleted. This action cannot be undone."}
        });
        DialogResult result = await dialogReference.Result;
        if(!result.Canceled)
        {
            await ProjectManagerService.DeleteAsync(new ProjectManagerService.DeleteRequestOptions(
                ProjectId: projectMeta.Id,
                Username: _username
            ));
            await UpdateProjectList();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnOpenClicked(ProjectMeta projectMeta)
    {
        await StateService.SetNetStateAsync(new Shared.Models.UiNetState(projectMeta));
        NavigationManager.NavigateTo($"net/upload/{projectMeta.Id}");
    }
}