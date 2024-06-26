using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Devices;

public partial class AddDeviceDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public IReadOnlyCollection<DeviceProviderMeta> DeviceProviders { get; init; } = Array.Empty<DeviceProviderMeta>();
    [Parameter] public string ServiceUniqueName { get; init; } = string.Empty;
    [Inject] IDeviceManagerService DeviceManagerService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;

    private DeviceProviderMeta _selectedProvider = null!;
    private string _deviceId = string.Empty;
    private string _devicePrefix = string.Empty;
    private string _providerError = string.Empty;
    private string _deviceError = string.Empty;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _selectedProvider = DeviceProviders.FirstOrDefault()!;
        OnSelectedProviderChanged();
    }

    private async void OnSelectedProviderChanged()
    {
        if (_selectedProvider != null)
        {
            _devicePrefix = _selectedProvider.Prefix;
        }

        await InvokeAsync(StateHasChanged);
    }

    private void OnCloseClicked()
    {
        MudDialog.Cancel();
    }

    private async void OnValidSubmit()
    {
        _providerError = string.Empty;
        _deviceError = string.Empty;
        if (_selectedProvider == null)
        {
            _providerError = "Please select a provider";
        }

        if (string.IsNullOrEmpty(_deviceId) || string.IsNullOrWhiteSpace(_deviceId))
        {
            _deviceError = "Please enter a device id";
        }

        if (DeviceProviders.Any(p => p.Devices.Any(d => d.Id.Equals(_deviceId))))
        {
            _deviceError = "Device with the same identifier already exists";
        }

        if (!string.IsNullOrEmpty(_providerError) || !string.IsNullOrEmpty(_deviceError))
        {
            return;
        }

        try
        {
            await DeviceManagerService.AddDeviceAsync(new DeviceManagerService.AddDeviceRequestOptions(ServiceUniqueName,
                                                                                                _selectedProvider!.Name,
                                                                                                _devicePrefix,
                                                                                                _deviceId));

            MudDialog.Close();
        }
        catch (Exception)
        {
            Snackbar.Add("Could not add Device", Severity.Warning);
        }
    }
}
