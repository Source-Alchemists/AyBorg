using AyBorg.Diagrams.Core.Models;
using AyBorg.SDK.Common.Models;
using AyBorg.Web.Pages.Agent.Editor.Nodes;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Toolbelt.Blazor.HotKeys2;

namespace AyBorg.Web.Pages.Agent.Shared;

public partial class StepDialog : ComponentBase, IDisposable
{
    private bool _disposedValue;
    private HotKeysContext _hotKeysContext = null!;

    [CascadingParameter] MudDialogInstance MudDialog { get; init; } = null!;
    [Parameter] public FlowNode Node { get; init; } = null!;
    [Inject] public HotKeys HotKeys { get; init; } = null!;

    private IReadOnlyCollection<FlowPort> _outputPorts = Array.Empty<FlowPort>();
    private IReadOnlyCollection<FlowPort> _inputPorts = Array.Empty<FlowPort>();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _hotKeysContext = HotKeys.CreateContext();
        _hotKeysContext.Add(ModCode.None, Code.Escape, CloseDialog, "Close dialog");

        _inputPorts = Node.Ports.Where(p => p.Alignment == PortAlignment.Left).Cast<FlowPort>().ToArray(); // Left for input
        _outputPorts = Node.Ports.Where(p => p.Alignment == PortAlignment.Right).Cast<FlowPort>().ToArray(); // Right for output
    }

    private void CloseDialog() => MudDialog.Close();

    private void OnCloseClicked() => CloseDialog();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _hotKeysContext.Dispose();
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
