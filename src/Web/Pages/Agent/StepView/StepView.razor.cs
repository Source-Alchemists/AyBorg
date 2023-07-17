using System.Collections.Immutable;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using AyBorg.Web.Pages.Agent.Editor.Nodes;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Toolbelt.Blazor.HotKeys2;

namespace AyBorg.Web.Pages.Agent.StepView;

#nullable disable

public partial class StepView : ComponentBase, IDisposable
{
    private bool _disposedValue;
    private HotKeysContext _hotKeysContext = null!;
    private string _serviceUniqueName;

    [Parameter] public string ServiceId { get; init; } = string.Empty;
    [Parameter] public string StepId { get; init; }
    [Inject] public HotKeys HotKeys { get; init; }
    [Inject] public IFlowService FlowService { get; init; }
    [Inject] public INotifyService NotifyService { get; init; }
    [Inject] IRegistryService RegistryService { get; init; }
    [Inject] NavigationManager NavigationManager { get; init; }

    private bool _isLoading = true;
    private Step _step = new();
    private ImmutableList<Port> _ports = ImmutableList.Create<Port>();
    private NotifyService.Subscription _iterationFinishedSubscription;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _hotKeysContext = HotKeys.CreateContext();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            IEnumerable<ServiceInfoEntry> services = await RegistryService!.ReceiveServicesAsync();
            ServiceInfoEntry service = services.FirstOrDefault(s => s.Id.ToString() == ServiceId);
            if (service == null)
            {
                return;
            }

            _serviceUniqueName = service.UniqueName;

            var stepGuid = Guid.Parse(StepId);
            _step = await FlowService.GetStepAsync(_serviceUniqueName, new Step { Id = stepGuid }, null, true, false, false);
            var flowPorts = new List<FlowPort>();

            _ports = _ports.Clear();
            _ports = _ports.AddRange(_step.Ports);

            await UpdateNode(null);

            if (_iterationFinishedSubscription != null) _iterationFinishedSubscription.Callback -= IterationFinishedNotificationReceived;
            _iterationFinishedSubscription = NotifyService.Subscribe(_serviceUniqueName, SDK.Communication.gRPC.NotifyType.AgentIterationFinished);
            _iterationFinishedSubscription.Callback += IterationFinishedNotificationReceived;
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async ValueTask UpdateNode(Guid? iterationId)
    {
        Step fullStep = await FlowService.GetStepAsync(_serviceUniqueName, _step, iterationId, true, false, false);

        foreach (Port port in fullStep.Ports)
        {
            if (port.Direction == PortDirection.Input)
            {
                Port inputPort = _ports.FirstOrDefault(p => p.Id.Equals(port.Id));
                if (inputPort == null || !_ports.Contains(inputPort)) continue;
                _ports = _ports.Replace(inputPort, port);
            }
            else if (port.Direction == PortDirection.Output)
            {
                Port outputPort = _ports.FirstOrDefault(p => p.Id.Equals(port.Id));
                if (outputPort == null || !_ports.Contains(outputPort)) continue;
                _ports = _ports.Replace(outputPort, port);
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    private static bool IsDisabled(Port port)
    {
        if(port.Direction == PortDirection.Output)
        {
            return true;
        }

        if(port.IsConnected)
        {
            return true;
        }

        return false;
    }

    private async void IterationFinishedNotificationReceived(object obj)
    {
        Guid iterationId = (Guid)obj;
        await UpdateNode(iterationId);
    }

    private void OnBackClicked()
    {
         NavigationManager.NavigateTo($"/agents/editor/{ServiceId}");
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
