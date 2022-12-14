using AyBorg.SDK.Data.DTOs;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Editor;

public partial class Editor : ComponentBase
{
    private string _baseUrl = string.Empty;
    private string _serviceUniqueName = string.Empty;
    private bool _hasServiceError = false;
    private ProjectMetaDto? _projectMeta;
    private bool _isProjectSaving = false;

    private bool _areSubComponentsHidden = true; // Workaround to update the flow with the correct Agent instance.

    /// <summary>
    /// Gets or sets the service identifier.
    /// </summary>
    [Parameter]
    public string ServiceId { get; set; } = string.Empty;

    [Inject] IRegistryService? RegistryService { get; set; }
    [Inject] IProjectManagementService? ProjectManagementService { get; set; }
    [Inject] IStateService StateService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            _areSubComponentsHidden = true;
            var services = await RegistryService!.ReceiveAllAvailableServicesAsync();

            var service = services.FirstOrDefault(s => s.Id.ToString() == ServiceId);
            if (service == null)
            {
                _hasServiceError = true;
                return;
            }

            _serviceUniqueName = service.UniqueName;

            _baseUrl = RegistryService.GetUrl(services, ServiceId);
            if (_baseUrl == string.Empty)
            {
                _hasServiceError = true;
                return;
            }

            await StateService.SetAgentStateAsync(new UiAgentState(service));

            _projectMeta = await ProjectManagementService!.GetActiveMetaAsync(_baseUrl);

            _areSubComponentsHidden = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnProjectSaveClicked()
    {
        if (_projectMeta == null)
        {
            throw new InvalidOperationException("Project meta is null");
        }

        _isProjectSaving = true;
        await ProjectManagementService!.TrySaveAsync(_baseUrl, _projectMeta);
        _isProjectSaving = false;
    }
}