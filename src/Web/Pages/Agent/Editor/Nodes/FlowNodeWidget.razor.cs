using System.Collections.Immutable;
using AyBorg.Diagrams.Core.Models;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared;
using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

#nullable disable

public partial class FlowNodeWidget : ComponentBase, IDisposable
{
    [Parameter] public FlowNode Node { get; init; }
    [Inject] IFlowService FlowService { get; init; }
    [Inject] IStateService StateService { get; init; }
    [Inject] NavigationManager NavigationManager { get; init; }
    private string NodeClass => Node.Selected ? "flow node box selected" : "flow node box";

    private ImmutableList<FlowPort> _outputPorts = ImmutableList.Create<FlowPort>();
    private ImmutableList<FlowPort> _inputPorts = ImmutableList.Create<FlowPort>();
    private bool _disposedValue;

    protected override Task OnInitializedAsync()
    {
        _inputPorts = _inputPorts.AddRange(Node.Ports.Where(p => p.Alignment == PortAlignment.Left).Cast<FlowPort>()); // Left for input
        _outputPorts = _outputPorts.AddRange(Node.Ports.Where(p => p.Alignment == PortAlignment.Right).Cast<FlowPort>()); // Right for output

        Node.StepChanged += OnNodeChangedAsync;
        FlowService.PortValueChanged += PortValueChanged;

        return base.OnInitializedAsync();
    }

    private async void OnNodeChangedAsync()
    {
        await InvokeAsync(StateHasChanged);
    }

    private void PortValueChanged(object sender, PortValueChangedEventArgs args)
    {
        FlowPort targetPort = _inputPorts.FirstOrDefault(p => p.Port.Id.Equals(args.Port.Id));
        targetPort?.Update(args.Port);
    }

    private static string GetPortClass(Port port)
    {
        string directionClass = port.Direction == PortDirection.Input ? " input" : " output";
        string typeClass = string.Empty;
        switch (port.Brand)
        {
            case PortBrand.String:
            case PortBrand.Folder:
            case PortBrand.Numeric:
            case PortBrand.Boolean:
            case PortBrand.StringCollection:
            case PortBrand.NumericCollection:
                typeClass = "field";
                break;
            case PortBrand.Image:
                typeClass = "image";
                break;
            case PortBrand.Rectangle:
            case PortBrand.RectangleCollection:
                typeClass = "shape";
                break;
        }
       ;
        return $"flow {directionClass} {typeClass}";
    }

    private void OnRemoveNodeClicked()
    {
        Node.Delete();
    }

    private void OnStepFullScreenClicked()
    {
        NavigationManager.NavigateTo($"/agents/editor/{StateService.AgentState.ServiceId}/step/{Node.Step.Id}");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Node.StepChanged -= OnNodeChangedAsync;
                FlowService.PortValueChanged -= PortValueChanged;
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
