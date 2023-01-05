using AyBorg.SDK.System.Runtime;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Shared;

#nullable disable

public partial class RuntimeToolbar : ComponentBase, IDisposable
{
    [Parameter]
    [EditorRequired]
    public string ServiceUniqueName { get; set; }

    [Inject] IRuntimeService RuntimeService { get; set; }

    [Inject] INotifyService NotifyService { get; set; }

    [Inject] ISnackbar Snackbar { get; set; }

    private string StopClass => _isAbortVisible ? "w50" : "mud-full-width";

    private EngineMeta _status = null;
    private bool _isFirstUpdate = true;
    private bool _isStopVisible = false;
    private bool _isAbortVisible = false;
    private bool _areButtonsDisabled = false;
    private bool _isButtonLoading = false;
    private NotifyService.Subscription _statusSubscription;
    private bool _disposedValue;

    protected override async Task OnParametersSetAsync()
    {
        _status = await RuntimeService.GetStatusAsync();
        UpdateButtonsState();
        if (_statusSubscription != null)
        {
            _statusSubscription.Callback -= StatusCallbackReceived;
            NotifyService.Unsubscribe(_statusSubscription);
        }
        _statusSubscription = NotifyService.Subscribe(ServiceUniqueName, SDK.Communication.gRPC.NotifyType.AgentEngineStateChanged);
        _statusSubscription.Callback += StatusCallbackReceived;
        await base.OnParametersSetAsync();
    }

    public async Task UpdateAsync()
    {
        await InvokeAsync(StateHasChanged);
    }

    private async void StatusCallbackReceived(object obj)
    {
        if (obj is EngineMeta engineMeta)
        {
            _status = engineMeta;
            UpdateButtonsState();
            await InvokeAsync(StateHasChanged);
        }
    }

    private void UpdateButtonsState()
    {
        _areButtonsDisabled = false;
        _isButtonLoading = false;
        if (_status == null && _isFirstUpdate)
        {
            _isStopVisible = false;
            _isAbortVisible = false;
            _isFirstUpdate = false;
            return;
        }

        _isFirstUpdate = false;

        if (_status == null && !_isFirstUpdate)
        {
            return;
        }

        switch (_status.State)
        {
            case EngineState.Idle:
            case EngineState.Stopped:
            case EngineState.Aborted:
            case EngineState.Finished:
                _isStopVisible = false;
                _isAbortVisible = false;
                break;
            case EngineState.Running:
                _isStopVisible = _status.ExecutionType == EngineExecutionType.ContinuousRun;
                _isAbortVisible = _status.ExecutionType == EngineExecutionType.SingleRun;
                break;
            case EngineState.Stopping:
                _isStopVisible = false;
                _isAbortVisible = true;
                break;
        }
    }

    private async Task OnSingleRunClicked()
    {
        _areButtonsDisabled = true;
        _isButtonLoading = true;
        if (await RuntimeService.StartRunAsync(EngineExecutionType.SingleRun) == null)
        {
            _status.State = EngineState.Idle;
            UpdateButtonsState();
            Snackbar.Add("Failed to start run", Severity.Warning);
        }
    }

    private async Task OnContinuousRunClicked()
    {
        _areButtonsDisabled = true;
        _isButtonLoading = true;
        if(await RuntimeService.StartRunAsync(EngineExecutionType.ContinuousRun) == null)
        {
            _status.State = EngineState.Idle;
            UpdateButtonsState();
            Snackbar.Add("Failed to start run", Severity.Warning);
        }
    }

    private async Task OnStopClicked()
    {
        _areButtonsDisabled = true;
        _isButtonLoading = true;
        if(await RuntimeService.StopRunAsync() == null)
        {
            _status.State = EngineState.Idle;
            UpdateButtonsState();
            Snackbar.Add("Failed to stop run", Severity.Warning);
        }
    }

    private async Task OnAbortClicked()
    {
        _areButtonsDisabled = true;
        _isButtonLoading = true;
        if (await RuntimeService.AbortRunAsync() == null)
        {
            _status.State = EngineState.Idle;
            UpdateButtonsState();
            Snackbar.Add("Failed to abort run", Severity.Warning);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_statusSubscription != null)
                {
                    _statusSubscription.Callback -= StatusCallbackReceived;
                    NotifyService.Unsubscribe(_statusSubscription);
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
