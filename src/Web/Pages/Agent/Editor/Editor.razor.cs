using AyBorg.Web.Pages.Agent.Projects;
using AyBorg.Web.Pages.Agent.Shared;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Editor;

public partial class Editor : ComponentBase
{
    private string _serviceUniqueName = string.Empty;
    private string _serviceName = string.Empty;
    private bool _hasServiceError = false;
    private ProjectMeta _projectMeta = new();
    private bool _isLoading = true;
    private bool _areSubComponentsHidden = true; // Workaround to update the flow with the correct Agent instance.
    private SDK.Projects.ProjectState _projectState => _projectMeta?.State ?? SDK.Projects.ProjectState.Draft;
    private bool _isProjectLoaded => _projectMeta?.IsActive ?? false;
    private string _projectName => _projectMeta?.Name ?? string.Empty;

    /// <summary>
    /// Gets or sets the service identifier.
    /// </summary>
    [Parameter] public string ServiceId { get; init; } = string.Empty;

    [Inject] IRegistryService? RegistryService { get; init; }
    [Inject] IProjectManagementService? ProjectManagementService { get; init; }
    [Inject] IStateService StateService { get; init; } = null!;
    [Inject] IDialogService DialogService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            _areSubComponentsHidden = true;
            await InvokeAsync(StateHasChanged);
            IEnumerable<ServiceInfoEntry> services = await RegistryService!.ReceiveServicesAsync();
            ServiceInfoEntry? service = services.FirstOrDefault(s => s.Id.ToString() == ServiceId);
            if (service == null)
            {
                _hasServiceError = true;
                return;
            }

            _serviceUniqueName = service.UniqueName;
            _serviceName = service.Name;

            await StateService.SetAgentStateAsync(new UiAgentState(service));

            _projectMeta = await ProjectManagementService!.GetActiveMetaAsync();

            _areSubComponentsHidden = false;
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnProjectSaveClicked()
    {
        _isLoading = true;
        if (!await ProjectManagementService!.TrySaveAsync(_projectMeta!, new ProjectSaveInfo
        {
            State = SDK.Projects.ProjectState.Draft,
            VersionName = _projectMeta.VersionName,
            Comment = string.Empty
        }))
        {
            Snackbar.Add("Could not save project.", Severity.Error);
        }
        _isLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void OnProjectSettingsClicked()
    {
        IDialogReference dialog = await DialogService.ShowAsync<ProjectSettingsDialog>("Project settings",
                                                                            new DialogParameters {
                                                                                { "ProjectMeta", _projectMeta }
                                                                            },
                                                                            new DialogOptions
                                                                            {
                                                                                MaxWidth = MaxWidth.Medium,
                                                                                FullWidth = true,
                                                                                CloseButton = true
                                                                            });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnCreateProjectClicked()
    {
        IDialogReference dialog = await DialogService.ShowAsync<CreateNewProjectDialog>("New project", new DialogOptions {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            _isLoading = true;
            await InvokeAsync(StateHasChanged);
            _projectMeta = await ProjectManagementService!.GetActiveMetaAsync();
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
