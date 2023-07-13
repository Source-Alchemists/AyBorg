using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication;
using AyBorg.SDK.Communication.MQTT;
using ImageTorque.Processing;
using Microsoft.Extensions.Logging;
using MQTTnet.Protocol;

namespace AyBorg.Plugins.MQTT
{
    public sealed class MqttClient : ICommunicationDevice, IDisposable
    {
        private readonly ILogger<ICommunicationDevice> _logger;
        private readonly StringPort _host = new("Host", PortDirection.Input, "localhost");
        private readonly NumericPort _port = new("Port", PortDirection.Input, 1883);
        private readonly EnumPort _qualityOfService = new("QoS", PortDirection.Input, MqttQualityOfServiceLevel.AtLeastOnce);
        private readonly BooleanPort _retain = new("Retain", PortDirection.Input, false);
        private readonly EnumPort _encoder = new("Encoder", PortDirection.Input, EncoderType.Jpeg);
        private readonly IMqttClientProviderFactory _clientProviderFactory;
        private readonly Dictionary<MessageSubscription, MqttSubscription> _subscriptions = new();
        private IMqttClientProvider _mqttClientProvider = null!;
        private bool _isConnected;
        private bool _isDisposed;

        public string Id { get; }

        public string Name { get; }

        public IReadOnlyCollection<string> Categories { get; } = new List<string> { "Communication", "MQTT" };

        public IEnumerable<IPort> Ports { get; }

        public MqttClient(ILogger<ICommunicationDevice> logger, IMqttClientProviderFactory clientProviderFactory, string id)
        {
            _logger = logger;
            _clientProviderFactory = clientProviderFactory;
            Id = id;
            Name = id;

            Ports = new List<IPort> {
            _host,
            _port,
            _qualityOfService,
            _retain,
            _encoder
        };
        }

        public async ValueTask<bool> TryConnectAsync()
        {
            try
            {
                _mqttClientProvider?.Dispose();
                _mqttClientProvider = _clientProviderFactory.Create(_logger, Name, _host.Value, Convert.ToInt32(_port.Value));
                await _mqttClientProvider.ConnectAsync();
                _isConnected = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Plugin), ex, "Failed connecting to MQTT broker");
                return false;
            }

            return true;
        }

        public async ValueTask<bool> TrySendAsync(string messageId, IPort port)
        {
            if (!_isConnected)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Not connected to MQTT broker");
                return false;
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
            if (!_isConnected)
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
                subscription.Received?.Invoke(new Message
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
            if (!_isConnected)
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
}
