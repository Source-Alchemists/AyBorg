using System.Collections.Immutable;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Modals;
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
    [Inject] ISnackbar Snackbar { get; init; } = null!;

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
        if (!result.Cancelled)
        {
            await UpdateProviderCollection();
        }
    }

    private async Task OnRemoveDeviceClicked(DeviceMeta deviceMeta)
    {
        IDialogReference dialogReference = DialogService.Show<ConfirmDialog>("Remove Device", new DialogParameters {
            { "ContentText", $"Are you sure you want to remove device '{deviceMeta.Name}'?"  }
        });
        DialogResult result = await dialogReference.Result;
        if (!result.Cancelled)
        {
            _isLoading = true;
            await InvokeAsync(StateHasChanged);
            try
            {
                DeviceMeta removedDevice = await DeviceManagerService.RemoveDeviceAsync(new DeviceManagerService.CommonDeviceRequestOptions(_serviceUniqueName, deviceMeta.Id));
                _devices = _devices.Remove(deviceMeta);
                _deviceProviders = await DeviceManagerService.GetDeviceProvidersAsync(_serviceUniqueName);
            }
            catch (Exception)
            {
                Snackbar.Add("Could not remove device", Severity.Warning);
            }
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnActivateDeviceClicked(DeviceMeta deviceMeta)
    {
        IDialogReference dialogReference = DialogService.Show<ConfirmDialog>("Activate Device", new DialogParameters {
            { "ContentText", $"Are you sure you want to activate device '{deviceMeta.Name}'?"  }
        });
        DialogResult result = await dialogReference.Result;
        if (!result.Cancelled)
        {
            await ChangeDeviceState(new DeviceManagerService.ChangeDeviceStateRequestOptions(_serviceUniqueName, deviceMeta.Id, true));
        }
    }

    private async Task OnDeactivateDeviceClicked(DeviceMeta deviceMeta)
    {
        IDialogReference dialogReference = DialogService.Show<ConfirmDialog>("Deactivate Device", new DialogParameters {
            { "ContentText", $"Are you sure you want to deactivate device '{deviceMeta.Name}'?"  }
        });
        DialogResult result = await dialogReference.Result;
        if (!result.Cancelled)
        {
            await ChangeDeviceState(new DeviceManagerService.ChangeDeviceStateRequestOptions(_serviceUniqueName, deviceMeta.Id, false));
        }
    }

    private async Task OnDeviceSettingsClicked(DeviceMeta deviceMeta)
    {
        IDialogReference dialogReference = DialogService.Show<DeviceSettingsDialog>($"{deviceMeta.Name} Settings", new DialogParameters {
            { "Device", deviceMeta }
        },
        new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });
        await dialogReference.Result;
        _isLoading = true;
        await InvokeAsync(StateHasChanged);
        await UpdateProviderCollection();
        _isLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private async ValueTask ChangeDeviceState(DeviceManagerService.ChangeDeviceStateRequestOptions options)
    {
        _isLoading = true;
        await InvokeAsync(StateHasChanged);
        DeviceMeta updatedDevice = await DeviceManagerService.ChangeDeviceStateAsync(options);
        DeviceMeta oldDevice = _devices.First(d => d.Id.Equals(updatedDevice.Id, StringComparison.InvariantCultureIgnoreCase));
        _devices = _devices.Replace(oldDevice, updatedDevice);
        _isLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private bool CanAdd(DeviceMeta deviceMeta)
    {
        DeviceProviderMeta provider = _deviceProviders.First(p => p.Devices.Any(d => d.Id.Equals(deviceMeta.Id, StringComparison.InvariantCultureIgnoreCase)));
        return provider.CanAdd;
    }
}
