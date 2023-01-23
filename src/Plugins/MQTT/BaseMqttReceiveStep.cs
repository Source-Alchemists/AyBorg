using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace AyBorg.Plugins.MQTT;

public abstract class BaseMqttReceiveStep : BaseMqttStep, IInitializable
{
    protected readonly NumericPort _timeoutMsPort = new("Timeout (ms)", PortDirection.Input, 10000, -1, int.MaxValue);
    protected string _lastTopic = string.Empty;
    protected MqttSubscription _subscription = null!;
    protected bool _hasNewMessage = false;

    protected BaseMqttReceiveStep(ILogger logger, IMqttClientProvider mqttClientProvider) : base(logger, mqttClientProvider)
    {
        _ports.Add(_topicPort);
        _ports.Add(_timeoutMsPort);
    }

    public async ValueTask OnInitializeAsync()
    {
        await SubcripeAsync();
    }

    public override async ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        _hasNewMessage = false;

        await SubcripeAsync();

        int count = 0;
        while (!_hasNewMessage && !cancellationToken.IsCancellationRequested)
        {
            // Dont add the cancellation token here, because we want to wait for the timeout
            // Else the MQTT client will be disposed
            await Task.Delay(1, default);
            count++;
            if (count > _timeoutMsPort.Value && _timeoutMsPort.Value != -1)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Result), "Timeout while waiting for message");
                return false;
            }
        }

        return true;
    }

    protected virtual async ValueTask SubcripeAsync()
    {
        if (_subscription != null && _lastTopic != _topicPort.Value)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(new EventId((int)EventLogType.PluginState), "Unsubscribing from topic {topic}", _lastTopic);
            }
            _subscription.MessageReceived -= OnMessageReceived;
            await _mqttClientProvider.UnsubscribeAsync(_subscription);
            _subscription = null!;
        }

        if (_subscription == null)
        {
            _subscription = await _mqttClientProvider.SubscribeAsync(_topicPort.Value);
            _subscription.MessageReceived += OnMessageReceived;
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(new EventId((int)EventLogType.PluginState), "Subscribed to topic {topic}", _topicPort.Value);
            }
        }

        _lastTopic = _topicPort.Value;
    }

    protected abstract void OnMessageReceived(MqttApplicationMessage message);
}
