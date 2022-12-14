using System.Text;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace AyBorg.Plugins.Base.MQTT;

public sealed class MqttReceive : BaseMqttReceiveStep
{
    private readonly StringPort _messagePort = new("Message", PortDirection.Output, string.Empty);

    public override string DefaultName => "MQTT.Receive";

    public MqttReceive(ILogger<MqttReceive> logger, IMqttClientProvider mqttClientProvider)
        : base(logger, mqttClientProvider)
    {
        _ports.Add(_messagePort);
    }

    protected override void OnMessageReceived(MqttApplicationMessage message)
    {
        if (message.Payload == null)
        {
            _logger.LogWarning("Received message with null payload");
            return;
        }

        _logger.LogTrace("Received message from topic {topic}", message.Topic);
        _messagePort.Value = Encoding.UTF8.GetString(message.Payload);
        _hasNewMessage = true;
    }
}