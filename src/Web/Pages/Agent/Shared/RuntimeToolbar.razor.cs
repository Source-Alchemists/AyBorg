using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using MQTTnet;
using Autodroid.SDK.Communication.MQTT;
using Autodroid.SDK.System.Runtime;
using Autodroid.Web.Services.Agent;


namespace Autodroid.Web.Pages.Agent.Shared;

#nullable disable

public partial class RuntimeToolbar : ComponentBase, IAsyncDisposable
{
    [Parameter]
    [EditorRequired]
    public string BaseUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public string ServiceUniqueName { get; set; }

    [Inject] IRuntimeService RuntimeService { get; set; }

    [Inject] IMqttClientProvider MqttClientProvider { get; set; }

    private string StopClass => _isAbortVisible ? "w50" : "mud-full-width";

    private EngineMeta _status = null;
    private bool _isFirstUpdate = true;
    private bool _isStopVisible = false;
    private bool _isAbortVisible = false;
    private bool _areButtonsDisabled = false;
    private bool _isButtonLoading = false;
    private MqttSubscription _statusSubscription;

    public async ValueTask DisposeAsync()
    {
        if (_statusSubscription != null)
        {
            _statusSubscription.MessageReceived -= OnMqttMessageReceived;
            await MqttClientProvider.UnsubscribeAsync(_statusSubscription);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrEmpty(BaseUrl)) return;
        _status = await RuntimeService.GetStatusAsync();
        UpdateButtonsState();
        if(_statusSubscription != null)
        {
            _statusSubscription.MessageReceived -= OnMqttMessageReceived;
            await MqttClientProvider.UnsubscribeAsync(_statusSubscription);
        }
        _statusSubscription = await MqttClientProvider.SubscribeAsync($"Autodroid/agents/{ServiceUniqueName}/engine/status");
        _statusSubscription.MessageReceived += OnMqttMessageReceived;

        await base.OnParametersSetAsync();
    }

    public async Task UpdateAsync()
    {
        await InvokeAsync(StateHasChanged);
    }

    private async void OnMqttMessageReceived(MqttApplicationMessage message)
    {
        if(message.Payload is null) return;
            var status = JsonSerializer.Deserialize<EngineMeta>(Encoding.UTF8.GetString(message.Payload));
            _status = status;
            UpdateButtonsState();
            await InvokeAsync(StateHasChanged);
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
        await RuntimeService.StartRunAsync(EngineExecutionType.SingleRun);
    }

    private async Task OnContinuousRunClicked()
    {
        _areButtonsDisabled = true;
        _isButtonLoading = true;
        await RuntimeService.StartRunAsync(EngineExecutionType.ContinuousRun);
    }

    private async Task OnStopClicked()
    {
        _areButtonsDisabled = true;
        _isButtonLoading = true;
        await RuntimeService.StopRunAsync();
    }

    private async Task OnAbortClicked()
    {
        _areButtonsDisabled = true;
        _isButtonLoading = true;
        if (await RuntimeService.AbortRunAsync() == null)
        {
            _status.State = EngineState.Idle;
            UpdateButtonsState();
        }
    }
}