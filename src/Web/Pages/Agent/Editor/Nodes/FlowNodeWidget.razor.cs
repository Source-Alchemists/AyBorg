using AyBorg.Diagrams.Core.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.Web.Pages.Agent.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

#nullable disable

public partial class FlowNodeWidget : ComponentBase, IDisposable
{
    [Parameter] public FlowNode Node { get; init; } = null!;
    [Inject] IDialogService DialogService { get; init; } = null!;
    private string NodeClass => Node.Selected ? "flow node box selected" : "flow node box";

    private IReadOnlyCollection<FlowPort> _outputPorts = Array.Empty<FlowPort>();
    private IReadOnlyCollection<FlowPort> _inputPorts = Array.Empty<FlowPort>();
    private bool _disposedValue;

    protected override Task OnInitializedAsync()
    {
        _inputPorts = Node.Ports.Where(p => p.Alignment == PortAlignment.Left).Cast<FlowPort>().ToArray(); // Left for input
        _outputPorts = Node.Ports.Where(p => p.Alignment == PortAlignment.Right).Cast<FlowPort>().ToArray(); // Right for output

        Node.StepChanged += OnChangedAsync;
        foreach (FlowPort ip in _inputPorts)
        {
            ip.PortChanged += OnChangedAsync;
        }

        return base.OnInitializedAsync();
    }

    private async void OnChangedAsync()
    {
        await InvokeAsync(StateHasChanged);
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

    private async void OnStepFullScreenClicked()
    {
        var dialogOptions = new DialogOptions
        {
            FullScreen = true,
            CloseButton = true,
            CloseOnEscapeKey = true,
            NoHeader = true
        };
        var dialogParameters = new DialogParameters
        {
            { "Node", Node }
        };
        IDialogReference dialog = DialogService.Show<StepDialog>($"Step: {Node.Step.Name}", dialogParameters, dialogOptions);
        DialogResult result = await dialog.Result;
        await InvokeAsync(StateHasChanged);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Node.StepChanged -= OnChangedAsync;
                foreach (FlowPort ip in _inputPorts)
                {
                    ip.PortChanged -= OnChangedAsync;
                }
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
