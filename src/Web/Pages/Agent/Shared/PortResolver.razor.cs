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

using AyBorg.Types.Models;
using AyBorg.Types.Ports;
using AyBorg.Web.Pages.Agent.Shared.Fields;
using AyBorg.Web.Services.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Shared;

public partial class PortResolver : ComponentBase
{
    [Parameter, EditorRequired] public PortModel Port { get; set; } = null!;
    [Parameter, EditorRequired] public IEnumerable<PortModel> Ports { get; init; } = Array.Empty<PortModel>();
    [Parameter] public bool Disabled { get; init; } = false;
    [Parameter] public bool OnlyThumbnail { get; init; } = false;
    [Parameter] public ListType Mode { get; init; } = ListType.Flow;
    [Parameter] public EventCallback<ValueChangedEventArgs> ValueChanged { get; set; }
    [Inject] IFlowService FlowService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;

    private IReadOnlyCollection<PortModel> _shapePorts = Array.Empty<PortModel>();

    protected override void OnParametersSet()
    {
        _shapePorts = Ports.Where(p => (p.Brand == PortBrand.Rectangle
                                    || p.Brand == PortBrand.RectangleCollection)
                                    && p.Direction == Port.Direction).ToArray();
        base.OnParametersSet();
    }

    private async Task OnPortValueChangedAsync(ValueChangedEventArgs e)
    {
        try
        {
            PortModel newPort = Port with { Value = e.Value };

            if (Mode == ListType.Flow)
            {
                if (await FlowService.TrySetPortValueAsync(newPort))
                {
                    Port = newPort;
                }
            }
            else
            {
                Port = newPort;
            }

            await ValueChanged.InvokeAsync(new ValueChangedEventArgs(newPort, newPort.Value));
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception)
        {
            Snackbar.Add("Could not set port value", Severity.Warning);
        }
    }

    public enum ListType
    {
        Flow,
        Device
    }
}
