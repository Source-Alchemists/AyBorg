using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Communication.MQTT;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging;
using MQTTnet.Protocol;

namespace AyBorg.Plugins.MQTT;

public sealed class MqttSend : BaseMqttSendStep, IStepBody
{
    private readonly StringPort _messagePort = new("Message", PortDirection.Input, string.Empty);
    public override string DefaultName => "MQTT.Send";

    public MqttSend(ILogger<MqttSend> logger, IMqttClientProvider mqttClientProvider, ICommunicationStateProvider communicationStateProvider)
        : base(logger, mqttClientProvider, communicationStateProvider)
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
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), e, "Error while sending message to MQTT");
            return false;
        }
    }
}
