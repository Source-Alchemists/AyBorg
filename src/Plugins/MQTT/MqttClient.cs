using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Communication;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.System.Runtime;
using ImageTorque.Processing;
using Microsoft.Extensions.Logging;
using MQTTnet.Protocol;

namespace AyBorg.Plugins.MQTT;

public sealed class MqttClient : ICommunicationDevice, IDisposable
{
    private readonly ILogger<ICommunicationDevice> _logger;
    private readonly StringPort _host = new("Host", PortDirection.Input, "localhost");
    private readonly NumericPort _port = new("Port", PortDirection.Input, 1883);
    private readonly EnumPort _qualityOfService = new("QoS", PortDirection.Input, MqttQualityOfServiceLevel.AtLeastOnce);
    private readonly BooleanPort _retain = new("Retain", PortDirection.Input, false);
    private readonly EnumPort _encoder = new("Encoder", PortDirection.Input, EncoderType.Jpeg);
    private readonly IMqttClientProviderFactory _clientProviderFactory;
    private readonly ICommunicationStateProvider _communicationStateProvider;
    private readonly Dictionary<MessageSubscription, MqttSubscription> _subscriptions = new();
    private IMqttClientProvider _mqttClientProvider = null!;
    private bool _isDisposed;

    public string Id { get; }

    public string Name { get; }

    public string Manufacturer => "Source Alchemists";

    public bool IsConnected { get; private set; }

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultDeviceCategories.Communication, "MQTT" };

    public IReadOnlyCollection<IPort> Ports { get; }

    public MqttClient(ILogger<ICommunicationDevice> logger, IMqttClientProviderFactory clientProviderFactory, ICommunicationStateProvider communicationStateProvider, string id)
    {
        _logger = logger;
        _clientProviderFactory = clientProviderFactory;
        _communicationStateProvider = communicationStateProvider;
        Id = id;
        Name = $"MQTT Client ({id})";

        Ports = new List<IPort> {
                _host,
                _port,
                _qualityOfService,
                _retain,
                _encoder
            };
    }

    public async ValueTask<bool> TryUpdate(IReadOnlyCollection<IPort> ports)
    {
        bool prevConnected = IsConnected;
        if (IsConnected && !await TryDisconnectAsync())
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Failed disconnecting from MQTT broker");
            return false;
        }

        foreach (IPort port in ports)
        {
            IPort? targetPort = Ports.FirstOrDefault(p => p.Id.Equals(port.Id) && p.Brand.Equals(port.Brand));
            if (targetPort == null)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Port {PortId} not found", port.Id);
                continue;
            }

            targetPort.UpdateValue(port);
        }

        if (prevConnected && !await TryConnectAsync())
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Failed connecting to MQTT broker");
            return false;
        }

        _logger.LogTrace(new EventId((int)EventLogType.Plugin), "Updated MQTT client");
        return true;
    }

    public async ValueTask<bool> TryConnectAsync()
    {
        try
        {
            _mqttClientProvider?.Dispose();
            _mqttClientProvider = _clientProviderFactory.Create(_logger, Name, _host.Value, Convert.ToInt32(_port.Value));
            await _mqttClientProvider.ConnectAsync();
            IsConnected = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), ex, "Failed connecting to MQTT broker");
            IsConnected = false;
        }

        return IsConnected;
    }

    public ValueTask<bool> TryDisconnectAsync()
    {
        try
        {
            _mqttClientProvider?.Dispose();
            IsConnected = false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), ex, "Failed disconnecting from MQTT broker");
            IsConnected = true;
        }

        return ValueTask.FromResult(!IsConnected);
    }

    public async ValueTask<bool> TrySendAsync(string messageId, IPort port)
    {
        if (!IsConnected)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Not connected to MQTT broker");
            return false;
        }

        if (!_communicationStateProvider.IsResultCommunicationEnabled)
        {
            _logger.LogTrace(new EventId((int)EventLogType.Plugin), "Communication is disabled");
            return true;
        }

        try
        {
            await _mqttClientProvider.PublishAsync(messageId, port, new MqttPublishOptions
            {
                QualityOfServiceLevel = (MqttQualityOfServiceLevel)_qualityOfService.Value,
                Retain = _retain.Value,
                EncoderType = (EncoderType)_encoder.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), ex, "Failed sending MQTT message");
            return false;
        }

        return true;
    }

    public async ValueTask<IMessageSubscription> SubscribeAsync(string messageId)
    {
        if (!IsConnected)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Not connected to MQTT broker");
            throw new InvalidOperationException("Not connected to MQTT broker");
        }

        MessageSubscription prevSub = _subscriptions.FirstOrDefault(p => p.Key.Id.Equals(messageId, StringComparison.InvariantCultureIgnoreCase)).Key;
        if (prevSub != null)
        {
            _logger.LogInformation(new EventId((int)EventLogType.Plugin), "Already subscribed to {messageId}", messageId);
            return prevSub;
        }

        MqttSubscription rawSub = await _mqttClientProvider.SubscribeAsync(messageId);
        var subscription = new MessageSubscription { Id = messageId };
        rawSub.MessageReceived += (message) =>
        {
            subscription.Next(new Message
            {
                ContentType = message.ContentType,
                Payload = message.Payload
            });
        };

        _subscriptions.Add(subscription, rawSub);

        return subscription;
    }

    public async ValueTask UnsubscribeAsync(IMessageSubscription subscription)
    {
        if (!IsConnected)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Not connected to MQTT broker");
            throw new InvalidOperationException("Not connected to MQTT broker");
        }

        MessageSubscription prevSub = _subscriptions.FirstOrDefault(p => p.Key.Id.Equals(subscription.Id, StringComparison.InvariantCultureIgnoreCase)).Key;
        if (prevSub == null)
        {
            _logger.LogInformation(new EventId((int)EventLogType.Plugin), "Not subscribed to {messageId}", subscription.Id);
            return;
        }

        MqttSubscription rawSub = _subscriptions[prevSub];
        await _mqttClientProvider.UnsubscribeAsync(rawSub);
        _ = _subscriptions.Remove(prevSub);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            _mqttClientProvider?.Dispose();
            _isDisposed = true;
        }
    }
}
