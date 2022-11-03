using Microsoft.Extensions.Logging;
using MQTTnet.Protocol;
using Atomy.SDK.Common.Ports;
using Atomy.SDK.ImageProcessing.Encoding;
using Atomy.SDK.Communication.MQTT;

namespace Atomy.Plugins.Base.MQTT;

public sealed class MqttImageSend : BaseMqttSendStep
{
    private readonly ImagePort _imagePort = new ImagePort("Image", PortDirection.Input, null!);
    private readonly EnumPort _encodingPort = new EnumPort("Encoding", PortDirection.Input, EncoderType.Jpeg);
    private readonly NumericPort _qualityPort = new NumericPort("Quality", PortDirection.Input, 80, 1, 100);

    public override string DefaultName => "MQTT.Image.Send";

    public MqttImageSend(ILogger<MqttImageSend> logger, IMqttClientProvider mqttClientProvider)
        : base(logger, mqttClientProvider)
    {
        _ports.Insert(0, _imagePort);
        _ports.Insert(1, _encodingPort);
        _ports.Insert(2, _qualityPort);
    }

    protected override async Task<bool> Send(CancellationToken cancellationToken)
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
            _logger.LogWarning(e, "Error while sending image to MQTT");
            return false;
        }
    }
}