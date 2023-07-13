using AyBorg.Web.Services;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Devices;

public partial class Devices : ComponentBase {
    private string _serviceUniqueName = string.Empty;
    private string _serviceName = string.Empty;
    private bool _hasServiceError = false;
    private bool _isLoading = true;
    [Parameter] public string ServiceId { get; init; } = string.Empty;
    [Inject] IRegistryService? RegistryService { get; init; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            IEnumerable<ServiceInfoEntry> services = await RegistryService!.ReceiveServicesAsync();
            ServiceInfoEntry? service = services.FirstOrDefault(s => s.Id.ToString() == ServiceId);
            if (service == null)
            {
                _hasServiceError = true;
                return;
            }

            _serviceUniqueName = service.UniqueName;
            _serviceName = service.Name;

            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private Task OnAddDeviceClicked()
    {
        return Task.CompletedTask;
    }
}
