using AyBorg.SDK.Data.DTOs;
using AyBorg.SDK.Projects;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using AyBorg.Web.Shared.Modals;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Projects;

public partial class Projects : ComponentBase
{
    private string _baseUrl = string.Empty;
    private bool _hasServiceError = false;
    private IEnumerable<ProjectMetaDto> _readyProjects = new List<ProjectMetaDto>();
    private IEnumerable<ProjectMetaDto> _reviewProjects = new List<ProjectMetaDto>();
    private IEnumerable<ProjectMetaDto> _draftProjects = new List<ProjectMetaDto>();

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
            IEnumerable<RegistryEntryDto> services = await RegistryService.ReceiveAllAvailableServicesAsync();
            _baseUrl = RegistryService.GetUrl(services, ServiceId);
            RegistryEntryDto? service = services.FirstOrDefault(s => s.Id.ToString() == ServiceId);
            if (_baseUrl == string.Empty || service == null)
            {
                _hasServiceError = true;
                return;
            }

            await StateService.SetAgentStateAsync(new UiAgentState(service));

            await ReceiveProjectsAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ReceiveProjectsAsync()
    {
        IEnumerable<ProjectMetaDto> allProjectMetas = await ProjectManagementService.GetMetasAsync(_baseUrl);
        _readyProjects = allProjectMetas.Where(p => p.State == ProjectState.Ready);
        _reviewProjects = allProjectMetas.Where(p => p.State == ProjectState.Review);
        _draftProjects = allProjectMetas.Where(p => p.State == ProjectState.Draft);
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnActivateProjectClicked(ProjectMetaDto projectDto)
    {
        if (await ProjectManagementService.TryActivateAsync(_baseUrl, projectDto))
        {
            await ReceiveProjectsAsync();
        }
    }

    private async void OnProjectDeleteClicked(ProjectMetaDto projectDto)
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters
        {
            { "ContentText", $"Are you sure you want to delete project '{projectDto.Name}'?" }
        };
        IDialogReference dialog = DialogService.Show<ConfirmDialog>("Delete project", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            if (await ProjectManagementService.TryDeleteAsync(_baseUrl, projectDto))
            {
                await ReceiveProjectsAsync();
            }
        }
    }

    private async void OnSaveAsReviewClicked(ProjectMetaDto projectDto)
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters
        {
            { "Project", projectDto }
        };
        IDialogReference dialog = DialogService.Show<CreateReviewProjectDialog>($"Create review for {projectDto.Name}", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            ProjectStateChangeDto stateChange = (ProjectStateChangeDto)result.Data;
            if (await ProjectManagementService.TrySaveNewVersionAsync(_baseUrl, projectDto.DbId, stateChange))
            {
                await ReceiveProjectsAsync();
            }
            else
            {
                Snackbar.Add("Failed to save new version", Severity.Error);
            }
        }
    }

    private async void OnAbandonReviewClicked(ProjectMetaDto projectDto)
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters
        {
            { "ContentText", $"Are you sure you want to abandon review for project '{projectDto.Name}'?" }
        };
        IDialogReference dialog = DialogService.Show<ConfirmDialog>("Abandon review", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            if (await ProjectManagementService.TrySaveNewVersionAsync(_baseUrl, projectDto.DbId, new ProjectStateChangeDto
            {
                State = ProjectState.Draft,
                VersionName = projectDto.VersionName,
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

    private async void OnSaveAsReadyClicked(ProjectMetaDto projectDto)
    {
        DialogParameters parameters = new()
        {
            { "Project", projectDto }
        };
        IDialogReference dialog = DialogService.Show<ConfirmProjectApproveDialog>($"Approve project {projectDto.Name}", parameters, new DialogOptions());
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            var resultProjectMetaDto = (ProjectMetaDto)result.Data;
            if (await ProjectManagementService.TryApproveAsnyc(_baseUrl, projectDto.DbId, new ProjectStateChangeDto
            {
                State = ProjectState.Draft,
                VersionName = projectDto.VersionName,
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
            { "BaseUrl", _baseUrl }
        };
        IDialogReference dialog = DialogService.Show<CreateNewProjectDialog>("New project", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            await ReceiveProjectsAsync();
        }
    }
}
