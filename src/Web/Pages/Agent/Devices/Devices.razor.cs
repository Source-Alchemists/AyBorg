using System.Collections.Immutable;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Devices;

public partial class Devices : ComponentBase
{
    private string _serviceUniqueName = string.Empty;
    private string _serviceName = string.Empty;
    private bool _hasServiceError = false;
    private bool _isLoading = true;
    private IReadOnlyCollection<DeviceProviderMeta> _deviceProviders = Array.Empty<DeviceProviderMeta>();
    private ImmutableList<DeviceMeta> _devices = ImmutableList.Create<DeviceMeta>();
    [Parameter] public string ServiceId { get; init; } = string.Empty;
    [Inject] IRegistryService RegistryService { get; init; } = null!;
    [Inject] IDialogService DialogService { get; init; } = null!;
    [Inject] IDeviceManagerService DeviceManagerService { get; init; } = null!;

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
            await UpdateProviderCollection();

            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task UpdateProviderCollection()
    {
        _deviceProviders = await DeviceManagerService.GetDeviceProvidersAsync(_serviceUniqueName);

        _devices = _devices.Clear();
        foreach (DeviceProviderMeta provider in _deviceProviders)
        {
            foreach (DeviceMeta device in provider.Devices)
            {
                _devices = _devices.Add(device);
            }
        }
    }

    private async Task OnAddDeviceClicked()
    {
        IDialogReference dialogReference = DialogService.Show<AddDeviceDialog>("Add Device", new DialogParameters
        {
            { "DeviceProviders", _deviceProviders },
            { "ServiceUniqueName", _serviceUniqueName }
        },
        new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });
        DialogResult result = await dialogReference.Result;
        if(!result.Cancelled)
        {
            await UpdateProviderCollection();
        }
    }
}
