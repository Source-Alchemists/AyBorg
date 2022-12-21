using AyBorg.SDK.Common.Models;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Grpc.Core;

namespace AyBorg.Web.Pages.Agent.Editor.Nodes;

#nullable disable

public class FlowNode : NodeModel, IDisposable
{
    private readonly IFlowService _flowService;
    private readonly IStateService _stateService;
    private readonly INotifyService _notifyService;
    private NotifyService.Subscription _subscription;
    private bool _disposedValue;

    public Step Step { get; private set; }

    /// <summary>
    /// Called when a step is updated.
    /// </summary>
    public Action StepChanged { get; set; }
    public Action OnDelete { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowNode"/> class.
    /// </summary>
    /// <param name="flowService">The flow service.</param>
    /// <param name="notifyService">The notify service.</param>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="step">The step.</param>
    public FlowNode(IFlowService flowService, INotifyService notifyService, IStateService stateService, Step step, bool locked = false) : base(new Point(step.X, step.Y))
    {
        _flowService = flowService;
        _notifyService = notifyService;
        _stateService = stateService;
        Title = step.Name;
        Step = step;
        Locked = locked;

        if (step.Ports == null) return;
        foreach (Port port in step.Ports)
        {
            _ = AddPort(new FlowPort(this, port, step, _flowService) { Locked = locked });
        }

        Subscribe();
    }

    private void Subscribe()
    {
        _subscription = _notifyService.CreateSubscription(_stateService.AgentState.UniqueName, SDK.Communication.gRPC.NotifyType.AgentIterationFinished);
        _subscription.Callback += NotificationReceived;
    }

    private async void NotificationReceived(object obj)
    {
        try
        {
            if (!Guid.TryParse(obj.ToString(), out Guid iterationId))
            {
                return;
            }
            Step step = await _flowService.GetStepAsync(Step.Id, iterationId);
            Step.ExecutionTimeMs = step.ExecutionTimeMs;
            foreach (FlowPort targetFlowPort in Ports.Cast<FlowPort>())
            {
                Port sourcePort = step.Ports.FirstOrDefault(p => p.Id.Equals(targetFlowPort.Port.Id));
                if (sourcePort == null)
                {
                    continue;
                }

                targetFlowPort.Port.Value = sourcePort.Value;
            }
            StepChanged?.Invoke();
        }
        catch (RpcException) { }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_subscription != null)
                {
                    _subscription.Callback -= NotificationReceived;
                    _notifyService.Unsubscribe(_subscription);
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

    public void Delete()
    {
        OnDelete?.Invoke();
    }
}
