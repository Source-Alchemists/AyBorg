using AyBorg.SDK.Common.Ports;
using AyBorg.Web.Pages.Agent.Editor.Nodes;
using AyBorg.Web.Pages.Agent.Shared.Fields;
using AyBorg.Web.Services.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Shared;

public partial class PortResolver : ComponentBase
{
    [Parameter, EditorRequired] public FlowPort Port { get; init; } = null!;
    [Parameter, EditorRequired] public IReadOnlyCollection<FlowPort> Ports { get; init; } = Array.Empty<FlowPort>();
    [Parameter] public bool AlternativeMode { get; init; } = false;
    [Inject] IFlowService FlowService { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;

    private IReadOnlyCollection<FlowPort> _shapePorts = Array.Empty<FlowPort>();

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
            FlowPort port = Ports.Cast<FlowPort>().First(p => p.Port.Id == e.Port.Id);
            SDK.Common.Models.Port newPort = port.Port with { Value = e.Value };
            port.Update(newPort);
            await FlowService.TrySetPortValueAsync(port.Port);
        }
        catch (Exception)
        {
            Snackbar.Add("Could not set port value", Severity.Warning);
        }
    }
}
