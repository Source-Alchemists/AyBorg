using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.System.Runtime;
using ImageTorque.Processing;
using Microsoft.Extensions.Logging;
using MQTTnet.Protocol;

namespace AyBorg.Plugins.MQTT;

public sealed class MqttImageSend : BaseMqttSendStep
{
    private readonly ImagePort _imagePort = new("Image", PortDirection.Input, null!);
    private readonly EnumPort _encodingPort = new("Encoding", PortDirection.Input, EncoderType.Jpeg);
    private readonly NumericPort _qualityPort = new("Quality", PortDirection.Input, 80, 1, 100);

    public override string DefaultName => "MQTT.Image.Send";

    public MqttImageSend(ILogger<MqttImageSend> logger, IMqttClientProvider mqttClientProvider, ICommunicationStateProvider communicationState)
        : base(logger, mqttClientProvider, communicationState)
    {
        _ports.Insert(0, _imagePort);
        _ports.Insert(1, _encodingPort);
        _ports.Insert(2, _qualityPort);
    }

    protected override async ValueTask<bool> Send(CancellationToken cancellationToken)
    {
        try
        {
            await _mqttClientProvider.PublishAsync(_topicPort.Value,
                                                _imagePort,
                                                new MqttPublishOptions
                                                {
                                                    QualityOfServiceLevel = (MqttQualityOfServiceLevel)_qosPort.Value,
                                                    Retain = _retainPort.Value,
                                                    EncoderType = (EncoderType)_encodingPort.Value,
                                                    Quality = Convert.ToInt32(_qualityPort.Value),
                                                }).ConfigureAwait(false);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), e, "Error while sending image to MQTT");
            return false;
        }
    }
}
