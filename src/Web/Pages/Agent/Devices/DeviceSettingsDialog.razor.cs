using System.Collections.Immutable;
using AyBorg.SDK.Common.Models;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Devices;

public partial class DeviceSettingsDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public DeviceMeta Device { get; init; } = null!;
    [Inject] IDeviceManagerService DeviceManagerService { get; init; } = null!;
    [Inject] IStateService StateService { get; init; } = null!;

    private bool _isLoading = true;
    private ImmutableList<Port> _ports = ImmutableList.Create<Port>();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            await UpdateDeviceAsync();
        }
    }

    private async ValueTask UpdateDeviceAsync()
    {
        _isLoading = false;
        _ports = _ports.Clear();
        await InvokeAsync(StateHasChanged);
        DeviceMeta fullDevice = await DeviceManagerService.GetDevice(new DeviceManagerService.CommonDeviceRequestOptions(StateService.AgentState.UniqueName, Device.Id));

        var tmpPorts = new List<Port>();
        foreach (Port port in fullDevice.Ports)
        {
            tmpPorts.Add(port);
        }

        _ports = _ports.AddRange(tmpPorts);
        _isLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private void OnCloseClicked()
    {
        MudDialog.Cancel();
    }
}
