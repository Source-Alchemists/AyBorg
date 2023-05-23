using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.Web.Pages.Agent.Editor.Nodes;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared;
using Microsoft.AspNetCore.Components;
using Toolbelt.Blazor.HotKeys2;

namespace AyBorg.Web.Pages.Agent.StepView;

#nullable disable

public partial class StepView : ComponentBase, IDisposable
{
    private bool _disposedValue;
    private HotKeysContext _hotKeysContext = null!;

    [Parameter] public string UniqueName { get; init; }
    [Parameter] public string StepId { get; init; }
    [Inject] public HotKeys HotKeys { get; init; }
    [Inject] public IFlowService FlowService { get; init; }
    [Inject] public INotifyService NotifyService { get; init; }

    private bool _isLoading = true;
    private Step _step = new();
    private FlowNode _node;
    private IReadOnlyCollection<FlowPort> _ports = Array.Empty<FlowPort>();
    private NotifyService.Subscription _iterationFinishedSubscription;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _hotKeysContext = HotKeys.CreateContext();
        FlowService.PortValueChanged += OnPortValueChangedAsync;
    }

    private async void OnPortValueChangedAsync(object sender, PortValueChangedEventArgs args)
    {
        FlowPort targetPort = _ports.FirstOrDefault(p => p.Port.Id.Equals(args.Port.Id));
        if (targetPort != null)
        {
            targetPort.Update(args.Port);
            await InvokeAsync(StateHasChanged);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            var stepGuid = Guid.Parse(StepId);
            _step = await FlowService.GetStepAsync(UniqueName, new Step { Id = stepGuid }, null, true, false, false);
            _node = new FlowNode(_step);
            var flowPorts = new List<FlowPort>();

            foreach (Port port in _step.Ports)
            {
                var newPort = new FlowPort(_node, port);
                flowPorts.Add(newPort);
            }
            _ports = flowPorts;

            await UpdateNode(null);

            if (_iterationFinishedSubscription != null) _iterationFinishedSubscription.Callback -= IterationFinishedNotificationReceived;
            _iterationFinishedSubscription = NotifyService.Subscribe(UniqueName, SDK.Communication.gRPC.NotifyType.AgentIterationFinished);
            _iterationFinishedSubscription.Callback += IterationFinishedNotificationReceived;
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async ValueTask UpdateNode(Guid? iterationId)
    {
        Step fullStep = await FlowService.GetStepAsync(UniqueName, _step, iterationId, true, false, false);

        foreach (Port port in fullStep.Ports)
        {
            if (port.Direction == PortDirection.Input)
            {
                FlowPort inputPort = _ports.FirstOrDefault(p => p.Port.Id.Equals(port.Id));
                if (inputPort == null) continue;
                inputPort.Update(port);
            }
            else if (port.Direction == PortDirection.Output)
            {
                FlowPort outputPort = _ports.FirstOrDefault(p => p.Port.Id.Equals(port.Id));
                if (outputPort == null) continue;
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
