using AyBorg.SDK.Common.Ports;
using AyBorg.Web.Pages.Agent.Shared.Fields;
using Blazor.Diagrams.Core.Models;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

public partial class FlowNodeWidget : ComponentBase, IAsyncDisposable
{
    [Parameter]
    public FlowNode Node { get; set; } = null!;
    private string NodeClass => Node.Selected ? "flow node box selected" : "flow node box";

    private IEnumerable<PortModel> _outputPorts = new List<PortModel>();
    private IEnumerable<PortModel> _inputPorts = new List<PortModel>();

    public ValueTask DisposeAsync()
    {
        Node.StepChanged -= OnChangedAsync;
        foreach (FlowPort ip in _inputPorts.Cast<FlowPort>())
        {
            ip.PortChanged -= OnChangedAsync;
        }
        Node.Dispose();
        return ValueTask.CompletedTask;
    }

    protected override Task OnInitializedAsync()
    {
        _inputPorts = Node.Ports.Where(p => p.Alignment == PortAlignment.Left); // Left for input
        _outputPorts = Node.Ports.Where(p => p.Alignment == PortAlignment.Right); // Right for output

        Node.StepChanged += OnChangedAsync;
        foreach (FlowPort ip in _inputPorts.Cast<FlowPort>())
        {
            ip.PortChanged += OnChangedAsync;
        }

        return base.OnInitializedAsync();
    }

    private async void OnChangedAsync()
    {
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnPortValueChangedAsync(ValueChangedEventArgs e)
    {
        FlowPort port = Node.Ports.Cast<FlowPort>().First(p => p.Port.Id == e.Port.Id);
        port.Port.Value = e.Value;
        await port.SendValueAsync();
    }

    private static string GetPortClass(PortModel port)
    {
        var fp = (FlowPort)port;
        string directionClass = fp.Direction == PortDirection.Input ? " input" : " output";
        string typeClass = string.Empty;
        switch (fp.Brand)
        {
            case PortBrand.String:
            case PortBrand.Folder:
            case PortBrand.Numeric:
                typeClass = "field";
                break;
            case PortBrand.Image:
                typeClass = "image";
                break;
            case PortBrand.Rectangle:
                typeClass = "shape";
                break;
        }
       ;
        return $"flow {directionClass} {typeClass}";
    }

    private void OnRemoveNode()
    {
        Node.Delete();
    }
}
