using AyBorg.Diagrams.Core.Models;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.Web.Pages.Agent.Editor.Nodes;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Toolbelt.Blazor.HotKeys2;

namespace AyBorg.Web.Pages.Agent.Shared;

#nullable disable

public partial class StepDialog : ComponentBase, IDisposable
{
    private bool _disposedValue;
    private HotKeysContext _hotKeysContext = null!;

    [CascadingParameter] MudDialogInstance MudDialog { get; init; }
    [Parameter] public FlowNode Node { get; init; }
    [Inject] public HotKeys HotKeys { get; init; }
    [Inject] public IFlowService FlowService { get; init; }
    [Inject] public INotifyService NotifyService { get; set; }
    [Inject] public IStateService StateService { get; set; }


    private IReadOnlyCollection<FlowPort> _ports = Array.Empty<FlowPort>();
    private NotifyService.Subscription _iterationFinishedSubscription;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _hotKeysContext = HotKeys.CreateContext();
        _hotKeysContext.Add(ModCode.None, Code.Escape, CloseDialog, "Close dialog");

        _ports = Node.Ports.Cast<FlowPort>().ToArray();

        if (_iterationFinishedSubscription != null) _iterationFinishedSubscription.Callback -= IterationFinishedNotificationReceived;
        _iterationFinishedSubscription = NotifyService.Subscribe(StateService.AgentState.UniqueName, SDK.Communication.gRPC.NotifyType.AgentIterationFinished);
        _iterationFinishedSubscription.Callback += IterationFinishedNotificationReceived;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            await UpdateNode(null);
        }
    }

    private async ValueTask UpdateNode(Guid? iterationId)
    {
        Step fullStep = await FlowService.GetStepAsync(Node.Step.Id, iterationId, true, false);

        foreach (Port port in fullStep.Ports)
        {
            if (port.Direction == PortDirection.Input)
            {
                FlowPort inputPort = _ports.FirstOrDefault(p => p.Port.Id.Equals(port.Id));
                if(inputPort == null) continue;
                inputPort.Update(port);
            }
            else if (port.Direction == PortDirection.Output)
            {
                FlowPort outputPort = _ports.FirstOrDefault(p => p.Port.Id.Equals(port.Id));
                if(outputPort == null) continue;
                outputPort.Locked = true;
                outputPort.Update(port);
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    private async void IterationFinishedNotificationReceived(object obj)
    {
        Guid iterationId = (Guid)obj;
        await UpdateNode(iterationId);
    }

    private void CloseDialog() => MudDialog.Close();

    private void OnCloseClicked() => CloseDialog();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_iterationFinishedSubscription != null) _iterationFinishedSubscription.Callback -= IterationFinishedNotificationReceived;
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
