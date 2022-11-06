using Microsoft.Extensions.Logging;
using Microsoft.IO;
using MQTTnet;
using Autodroid.SDK.ImageProcessing;
using Autodroid.SDK.Common.Ports;
using Autodroid.SDK.Communication.MQTT;

namespace Autodroid.Plugins.Base.MQTT;

public sealed class MqttImageReceive : BaseMqttReceiveStep
{
    private readonly ImagePort _imagePort = new("Image", PortDirection.Output, null!);
    private readonly RecyclableMemoryStreamManager _memoryStreamManager = new();

    public override string DefaultName => "MQTT.Image.Receive";

    public  MqttImageReceive(ILogger<MqttImageReceive> logger, IMqttClientProvider mqttClientProvider)
        : base(logger, mqttClientProvider)
    {
        _ports.Add(_imagePort);
    }

    protected override void OnMessageReceived(MqttApplicationMessage message)
    {
        if(message.Payload == null)
        {
            _logger.LogWarning("Received message with null payload");
            return;
        }

        _logger.LogTrace("Received message from topic {topic}", message.Topic);
        using var stream = _memoryStreamManager.GetStream(message.Payload);

        var image = Image.Load(stream);
        _imagePort.Value?.Dispose();
        _imagePort.Value = image;
        _hasNewMessage = true;
    }
}