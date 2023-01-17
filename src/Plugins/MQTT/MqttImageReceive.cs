using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using ImageTorque;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using MQTTnet;

namespace AyBorg.Plugins.MQTT;

public sealed class MqttImageReceive : BaseMqttReceiveStep
{
    private readonly ImagePort _imagePort = new("Image", PortDirection.Output, null!);
    private readonly RecyclableMemoryStreamManager _memoryStreamManager = new();

    public override string DefaultName => "MQTT.Image.Receive";

    public MqttImageReceive(ILogger<MqttImageReceive> logger, IMqttClientProvider mqttClientProvider)
        : base(logger, mqttClientProvider)
    {
        _ports.Add(_imagePort);
    }

    protected override void OnMessageReceived(MqttApplicationMessage message)
    {
        if (message.Payload == null)
        {
            _logger.LogWarning("Received message with null payload");
            return;
        }

        _logger.LogTrace("Received message from topic {topic}", message.Topic);
        using MemoryStream stream = _memoryStreamManager.GetStream(message.Payload);

        var image = Image.Load(stream);
        _imagePort.Value?.Dispose();
        _imagePort.Value = image;
        _hasNewMessage = true;
    }
}
