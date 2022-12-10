using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using AyBorg.Web.Shared.Modals;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Projects;

public partial class Projects : ComponentBase
{
    private string _serviceUniqueName = string.Empty;
    private string _serviceName = string.Empty;
    private bool _hasServiceError = false;
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
            _serviceUniqueName = service.UniqueName;

            await StateService.SetAgentStateAsync(new UiAgentState(service));

            await ReceiveProjectsAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ReceiveProjectsAsync()
    {
        IEnumerable<ProjectMeta> allProjectMetas = await ProjectManagementService.GetMetasAsync(_serviceUniqueName);
        _readyProjects = allProjectMetas.Where(p => p.State == SDK.Projects.ProjectState.Ready);
        _reviewProjects = allProjectMetas.Where(p => p.State == SDK.Projects.ProjectState.Review);
        _draftProjects = allProjectMetas.Where(p => p.State == SDK.Projects.ProjectState.Draft);
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnActivateProjectClicked(ProjectMeta project)
    {
        if (await ProjectManagementService.TryActivateAsync(_serviceUniqueName, project))
        {
            await ReceiveProjectsAsync();
        }
    }

    private async void OnProjectDeleteClicked(ProjectMeta project)
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters
        {
            { "ContentText", $"Are you sure you want to delete project '{project.Name}'?" }
        };
        IDialogReference dialog = DialogService.Show<ConfirmDialog>("Delete project", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            if (await ProjectManagementService.TryDeleteAsync(_serviceUniqueName, project))
            {
                await ReceiveProjectsAsync();
            }
        }
    }

    private async void OnSaveAsReviewClicked(ProjectMeta projectMeta)
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters
        {
            { "Project", projectMeta }
        };
        IDialogReference dialog = DialogService.Show<CreateReviewProjectDialog>($"Create review for {projectMeta.Name}", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            ProjectSaveInfo stateChange = (ProjectSaveInfo)result.Data;
            if (await ProjectManagementService.TrySaveAsync(_serviceUniqueName, projectMeta, stateChange))
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
            { "ContentText", $"Are you sure you want to abandon review for project '{projectMeta.Name}'?" }
        };
        IDialogReference dialog = DialogService.Show<ConfirmDialog>("Abandon review", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            if (await ProjectManagementService.TrySaveAsync(_serviceUniqueName, projectMeta, new ProjectSaveInfo
            {
                State = SDK.Projects.ProjectState.Draft,
                VersionName = projectMeta.VersionName,
                Comment = "Abandoned review"
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
        DialogParameters parameters = new()
        {
            { "Project", projectMeta }
        };
        IDialogReference dialog = DialogService.Show<ConfirmProjectApproveDialog>($"Approve project {projectMeta.Name}", parameters, new DialogOptions());
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            var resultProjectMetaDto = (ProjectMeta)result.Data;
            if (await ProjectManagementService.TryApproveAsync(_serviceUniqueName, projectMeta.DbId, new ProjectSaveInfo
            {
                State = SDK.Projects.ProjectState.Draft,
                VersionName = projectMeta.VersionName,
                Comment = resultProjectMetaDto.Comment,
                UserName = resultProjectMetaDto.ApprovedBy
            }))
            {
                await ReceiveProjectsAsync();
            }
        }
    }

    private async Task OnNewProjectClicked()
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters
        {
            { "BaseUrl", _serviceUniqueName }
        };
        IDialogReference dialog = DialogService.Show<CreateNewProjectDialog>("New project", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            await ReceiveProjectsAsync();
        }
    }
}
