using MQTTnet.Protocol;
using Microsoft.Extensions.Logging;
using Atomy.SDK.Common.Ports;
using Atomy.SDK.Communication.MQTT;
using Atomy.SDK.Common;

namespace Atomy.Plugins.Base.MQTT;

public sealed class MqttSend : BaseMqttSendStep, IStepBody
{
    private readonly StringPort _messagePort = new("Message", PortDirection.Input, string.Empty);
    public override string DefaultName => "MQTT.Send";

    public MqttSend(ILogger<MqttSend> logger, IMqttClientProvider mqttClientProvider)
        : base(logger, mqttClientProvider)
    {
        _ports.Insert(0, _messagePort);
    }

    protected override async Task<bool> Send(CancellationToken cancellationToken)
    {
        try
        {
            await _mqttClientProvider.PublishAsync(_topicPort.Value,
                                                    _messagePort,
                                                    new MqttPublishOptions
                                                    {
                                                        QualityOfServiceLevel = (MqttQualityOfServiceLevel)_qosPort.Value,
                                                        Retain = _retainPort.Value
                                                    }).ConfigureAwait(false);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error while sending message to MQTT");
            return false;
        }
    }
}