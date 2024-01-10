using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.Web.Pages.Agent.Shared.Fields;
using AyBorg.Web.Services.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Shared;

public partial class PortResolver : ComponentBase
{
    [Parameter, EditorRequired] public Port Port { get; set; } = null!;
    [Parameter, EditorRequired] public IEnumerable<Port> Ports { get; init; } = Array.Empty<Port>();
    [Parameter] public bool Disabled { get; init; } = false;
    [Parameter] public bool OnlyThumbnail { get; init; } = false;
    [Parameter] public ListType Mode { get; init; } = ListType.Flow;
    [Parameter] public EventCallback<ValueChangedEventArgs> ValueChanged { get; set; }
    [Inject] IFlowService FlowService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;

    private IReadOnlyCollection<Port> _shapePorts = Array.Empty<Port>();

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
            Port newPort = Port with { Value = e.Value };

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
