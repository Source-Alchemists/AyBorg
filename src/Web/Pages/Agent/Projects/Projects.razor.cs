using Microsoft.AspNetCore.Components;
using Atomy.SDK.Data.DTOs;
using Atomy.Web.Services;
using Atomy.Web.Services.Agent;
using Atomy.Web.Shared.Modals;
using MudBlazor;
using Atomy.Web.Shared.Models;
using Atomy.SDK.Projects;

namespace Atomy.Web.Pages.Agent.Projects;

public partial class Projects : ComponentBase
{
    private string _baseUrl = string.Empty;
    private bool _hasServiceError = false;
    private ServiceRegistryEntryDto _serviceDetails = new ServiceRegistryEntryDto();
    private IEnumerable<ProjectMetaDto> _readyProjects = new List<ProjectMetaDto>();
    private IEnumerable<ProjectMetaDto> _draftProjects = new List<ProjectMetaDto>();

    [Parameter]
    public string ServiceId { get; set; } = string.Empty;

    [Inject] IRegistryService RegistryService { get; set; } = null!;
    [Inject] IProjectManagementService ProjectManagementService { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;
    [Inject] IStateService StateService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            var services = await RegistryService.ReceiveAllAvailableServicesAsync();
            _baseUrl = RegistryService.GetUrl(services, ServiceId);
            var service = services.FirstOrDefault(s => s.Id.ToString() == ServiceId);
            if (_baseUrl == string.Empty || service == null)
            {
                _hasServiceError = true;
                return;
            }

            StateService.AgentState = new UiAgentState(service);
            await StateService.RefreshAsync();

            await ReceiveProjectsAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ReceiveProjectsAsync()
    {
        var allProjectMetas = await ProjectManagementService.GetMetasAsync(_baseUrl);
        _readyProjects = allProjectMetas.Where(p => p.State == ProjectState.Ready);
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
        var parameters = new DialogParameters();
        parameters.Add("ContentText", $"Are you sure you want to delete project '{projectDto.Name}'?");
        var dialog = DialogService.Show<ConfirmDialog>("Delete project", parameters, options);
        var result = await dialog.Result;
        if(!result.Cancelled)
        {
            if (await ProjectManagementService.TryDeleteAsync(_baseUrl, projectDto))
            {
                await ReceiveProjectsAsync();
            }
        }
    }

    private async void OnSaveAsReadyClicked(ProjectMetaDto projectDto)
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters();
        parameters.Add("ContentText", $"Are you sure you want to save project '{projectDto.Name}' as ready?");
        var dialog = DialogService.Show<ConfirmDialog>("Save project as ready", parameters, options);
        var result = await dialog.Result;
        if(!result.Cancelled)
        {
            if (await ProjectManagementService.TrySaveAsReadyAsync(_baseUrl, projectDto))
            {
                await ReceiveProjectsAsync();
            }
        }
    }

    private async Task OnNewProjectClicked()
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters();
        parameters.Add("BaseUrl", _baseUrl);
        var dialog = DialogService.Show<CreateNewProjectDialog>("New project", parameters, options);
        var result = await dialog.Result;
        if(!result.Cancelled)
        {
            await ReceiveProjectsAsync();
        }
    }
}