using Autodroid.SDK.Common;
using Autodroid.SDK.Common.Ports;
using Autodroid.SDK.Communication.MQTT;
using Microsoft.Extensions.Logging;
using MQTTnet.Protocol;

namespace Autodroid.Plugins.Base.MQTT;

public sealed class MqttSend : BaseMqttSendStep, IStepBody
{
    private readonly StringPort _messagePort = new("Message", PortDirection.Input, string.Empty);
    public override string DefaultName => "MQTT.Send";

    public MqttSend(ILogger<MqttSend> logger, IMqttClientProvider mqttClientProvider)
        : base(logger, mqttClientProvider)
    {
        _ports.Insert(0, _messagePort);
    }

    protected override async ValueTask<bool> Send(CancellationToken cancellationToken)
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