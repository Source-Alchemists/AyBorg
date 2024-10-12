/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Immutable;
using AyBorg.Types;
using AyBorg.Types.Models;
using AyBorg.Web.Pages.Agent.Shared.Fields;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Modals;
using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Devices;

#nullable disable

public partial class DeviceSettingsDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public DeviceMeta Device { get; set; }
    [Inject] IDeviceManagerService DeviceManagerService { get; init; }
    [Inject] IStateService StateService { get; init; }
    [Inject] ISnackbar Snackbar { get; init; }
    [Inject] IDialogService DialogService { get; init; }
    [Inject] ILogger<DeviceSettingsDialog> Logger { get; init; }

    private bool _isLoading = true;
    private bool _isDisabled => Device.IsConnected;
    private ImmutableList<PortModel> _ports = [];

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
        DeviceMeta fullDevice = await DeviceManagerService.GetDeviceAsync(new DeviceManagerService.CommonDeviceRequestOptions(StateService.AgentState.UniqueName, Device.Id));

        var tmpPorts = new List<PortModel>();
        foreach (PortModel port in fullDevice.Ports)
        {
            tmpPorts.Add(port);
        }

        _ports = _ports.AddRange(tmpPorts);
        _isLoading = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnPortValueChangedAsync(ValueChangedEventArgs e)
    {
        PortModel oldPort = _ports.First(p => e.Port.Id.Equals(p.Id));
        _ports = _ports.Replace(oldPort, e.Port);
        Logger.LogInformation((int)EventLogType.UserInteraction, "Port [{portName}] changed to [{portValue}]", e.Port.Name, e.Port.Value);
        await InvokeAsync(StateHasChanged);
    }

    private void OnCloseClicked()
    {
        MudDialog.Cancel();
    }

    private async void OnApplyClicked()
    {
        _isLoading = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            await UpdateDevicePortsAsync();
            Snackbar.Add("Device settings updated", Severity.Success);
        }
        catch (Exception)
        {
            Snackbar.Add("Failed to update device settings", Severity.Warning);
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnConnectClicked()
    {
        IDialogReference dialogReference = await DialogService.ShowAsync<ConfirmDialog>("Confirm", new DialogParameters
        {
            { "ContentText", "Are you sure you want to apply settings and connect this device?" }
        });
        DialogResult result = await dialogReference.Result;
        if (result.Canceled)
        {
            return;
        }

        _isLoading = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            await UpdateDevicePortsAsync();
            DeviceMeta updatedDevice = await DeviceManagerService.ChangeDeviceStateAsync(new DeviceManagerService.ChangeDeviceStateRequestOptions(StateService.AgentState.UniqueName, Device.Id, true));
            if (!updatedDevice.IsActive)
            {
                Snackbar.Add("Could not connect device", Severity.Warning);
            }

            Device = updatedDevice;
            if (Device.IsConnected)
            {
                Snackbar.Add("Device connected", Severity.Success);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Could not connect device", Severity.Warning);
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void OnDisconnectClicked()
    {
        IDialogReference dialogReference = await DialogService.ShowAsync<ConfirmDialog>("Confirm", new DialogParameters
        {
            { "ContentText", "Are you sure you want disconnect this device?" }
        });
        DialogResult result = await dialogReference.Result;
        if (result.Canceled)
        {
            return;
        }
        _isLoading = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            DeviceMeta updatedDevice = await DeviceManagerService.ChangeDeviceStateAsync(new DeviceManagerService.ChangeDeviceStateRequestOptions(StateService.AgentState.UniqueName, Device.Id, false));
            if (updatedDevice.IsActive)
            {
                Snackbar.Add("Could not disconnect device", Severity.Warning);
            }

            Device = updatedDevice;
        }
        catch (Exception)
        {
            Snackbar.Add("Could not disconnect device", Severity.Warning);
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async ValueTask UpdateDevicePortsAsync()
    {
        DeviceMeta newDevice = await DeviceManagerService.UpdateDeviceAsync(new DeviceManagerService.UpdateDeviceRequestOptions(StateService.AgentState.UniqueName, Device.Id, _ports));
        var tmpPorts = new List<PortModel>();
        foreach (PortModel port in newDevice.Ports)
        {
            tmpPorts.Add(port);
        }

        _ports = _ports.Clear();
        _ports = _ports.AddRange(tmpPorts);
    }
}
