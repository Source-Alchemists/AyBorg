using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Modals;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Projects;

public partial class Projects : ComponentBase
{
    private string _serviceName = string.Empty;
    private bool _hasServiceError = false;
    private bool _isLoading = true;
    private IEnumerable<ProjectMeta> _readyProjects = new List<ProjectMeta>();
    private IEnumerable<ProjectMeta> _reviewProjects = new List<ProjectMeta>();
    private IEnumerable<ProjectMeta> _draftProjects = new List<ProjectMeta>();

    [Parameter]
    public string ServiceId { get; set; } = string.Empty;

    [Inject] IRegistryService RegistryService { get; set; } = null!;
    [Inject] IProjectManagementService ProjectManagementService { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;
    [Inject] IStateService StateService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            IEnumerable<ServiceInfoEntry> services = await RegistryService.ReceiveServicesAsync();
            ServiceInfoEntry? service = services.FirstOrDefault(s => s.Id.ToString() == ServiceId);
            if (service == null)
            {
                _hasServiceError = true;
                return;
            }

            _serviceName = service.Name;

            await StateService.SetAgentStateAsync(new UiAgentState(service));

            await ReceiveProjectsAsync();
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ReceiveProjectsAsync()
    {
        IEnumerable<ProjectMeta> allProjectMetas = await ProjectManagementService.GetMetasAsync();
        _readyProjects = allProjectMetas.Where(p => p.State == SDK.Projects.ProjectState.Ready);
        _reviewProjects = allProjectMetas.Where(p => p.State == SDK.Projects.ProjectState.Review);
        _draftProjects = allProjectMetas.Where(p => p.State == SDK.Projects.ProjectState.Draft);
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnActivateProjectClicked(ProjectMeta project)
    {
        if (await ProjectManagementService.TryActivateAsync(project))
        {
            await ReceiveProjectsAsync();
        }
    }

    private async void OnProjectDeleteClicked(ProjectMeta project)
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters
        {
            { "NeedPassword", true },
            { "ContentText", $"Are you sure you want to delete project '{project.Name}'? This action cannot be undone." }
        };
        IDialogReference dialog = DialogService.Show<ConfirmDialog>("Delete Project", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled && await ProjectManagementService.TryDeleteAsync(project))
        {
            await ReceiveProjectsAsync();
        }
    }

    private async void OnSaveAsReviewClicked(ProjectMeta projectMeta)
    {
        IDialogReference dialog = DialogService.Show<ConfirmDialog>($"Create Review for {projectMeta.Name}",
        new DialogParameters {
            { "ShowComment", true },
            { "Comment", projectMeta.Comment },
            { "ShowVersion", true },
            { "Version", projectMeta.VersionName },
            { "NeedPassword", true }
        },
        new DialogOptions {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            ConfirmDialog.ConfirmResult dialogResult = (ConfirmDialog.ConfirmResult)result.Data;
            if (await ProjectManagementService.TrySaveAsync(projectMeta, new ProjectSaveInfo {
                State = projectMeta.State,
                VersionName = dialogResult.Version,
                Comment = dialogResult.Comment
            }))
            {
                await ReceiveProjectsAsync();
            }
            else
            {
                Snackbar.Add("Failed to save new version", Severity.Error);
            }
        }
    }

    private async void OnAbandonReviewClicked(ProjectMeta projectMeta)
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters
        {
            { "ContentText", $"Are you sure you want to abandon review for project '{projectMeta.Name}'?" },
            { "NeedPassword", true }
        };
        IDialogReference dialog = DialogService.Show<ConfirmDialog>("Abandon Review", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            if (await ProjectManagementService.TrySaveAsync(projectMeta, new ProjectSaveInfo
            {
                State = SDK.Projects.ProjectState.Draft,
                VersionName = projectMeta.VersionName,
                Comment = "Abandoned Review"
            }))
            {
                await ReceiveProjectsAsync();
            }
            else
            {
                Snackbar.Add("Failed to abandon review", Severity.Error);
            }
        }
    }

    private async void OnSaveAsReadyClicked(ProjectMeta projectMeta)
    {
        IDialogReference dialog = DialogService.Show<ConfirmDialog>($"Approve Project {projectMeta.Name}",
        new DialogParameters {
            { "ShowComment", true },
            { "Comment", projectMeta.Comment },
            { "NeedPassword", true }
        },
        new DialogOptions {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            var dialogResult = (ConfirmDialog.ConfirmResult)result.Data;
            if (await ProjectManagementService.TryApproveAsync(projectMeta, new ProjectSaveInfo
            {
                State = SDK.Projects.ProjectState.Draft,
                VersionName = projectMeta.VersionName,
                Comment = dialogResult.Comment,
                UserName = dialogResult.UserName
            }))
            {
                await ReceiveProjectsAsync();
            }
        }
    }

    private async Task OnNewProjectClicked()
    {
        IDialogReference dialog = DialogService.Show<CreateNewProjectDialog>("New Project", new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            await ReceiveProjectsAsync();
        }
    }
}
