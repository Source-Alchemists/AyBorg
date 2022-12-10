using AyBorg.SDK.Data.DTOs;
using AyBorg.Web.Pages.Agent.Shared;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Editor;

public partial class Editor : ComponentBase
{
    private string _baseUrl = string.Empty;
    private string _serviceUniqueName = string.Empty;
    private string _serviceName = string.Empty;
    private bool _hasServiceError = false;
    private ProjectMetaDto? _projectMeta;
    private bool _isProjectServerWaiting = false;

    private bool _areSubComponentsHidden = true; // Workaround to update the flow with the correct Agent instance.

    /// <summary>
    /// Gets or sets the service identifier.
    /// </summary>
    [Parameter]
    public string ServiceId { get; set; } = string.Empty;

    [Inject] IRegistryService? RegistryService { get; set; }
    [Inject] IProjectManagementService? ProjectManagementService { get; set; }
    [Inject] IStateService StateService { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            _isProjectServerWaiting = true;
            _areSubComponentsHidden = true;
            IEnumerable<ServiceInfoEntry> services = await RegistryService!.ReceiveServicesAsync();

            ServiceInfoEntry? service = services.FirstOrDefault(s => s.Id.ToString() == ServiceId);
            if (service == null)
            {
                _hasServiceError = true;
                return;
            }

            _serviceUniqueName = service.UniqueName;
            _serviceName = service.Name;

            _baseUrl = RegistryService.GetUrl(services, ServiceId);
            if (_baseUrl == string.Empty)
            {
                _hasServiceError = true;
                return;
            }

            await StateService.SetAgentStateAsync(new UiAgentState(service));

            _projectMeta = await ProjectManagementService!.GetActiveMetaAsync(_baseUrl);

            _areSubComponentsHidden = false;
            _isProjectServerWaiting = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnProjectSaveClicked()
    {
        _isProjectServerWaiting = true;
        await ProjectManagementService!.TrySaveAsync(_baseUrl, _projectMeta!);
        _isProjectServerWaiting = false;
        await InvokeAsync(StateHasChanged);
    }

    private async void OnProjectSettingsClicked()
    {
        IDialogReference dialog = DialogService.Show<ProjectSettingsDialog>("Project settings",
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
        if (!result.Cancelled)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}
