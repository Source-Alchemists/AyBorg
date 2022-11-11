using Autodroid.SDK.Common.Ports;
using Autodroid.SDK.Communication.MQTT;
using Microsoft.Extensions.Logging;
using MQTTnet.Protocol;

namespace Autodroid.Plugins.Base.MQTT;

public abstract class BaseMqttSendStep : BaseMqttStep, IDisposable
{
    private Task _parallelTask = null!;
    protected readonly EnumPort _qosPort = new("QoS", PortDirection.Input, MqttQualityOfServiceLevel.AtMostOnce);
    protected readonly BooleanPort _retainPort = new("Retain", PortDirection.Input, false);
    protected readonly BooleanPort _parallelPort = new("Parallel", PortDirection.Input, false);
    private bool disposedValue;


    public BaseMqttSendStep(ILogger logger, IMqttClientProvider mqttClientProvider)
        : base(logger, mqttClientProvider)
    {
        _ports.Add(_topicPort);
        _ports.Add(_qosPort);
        _ports.Add(_retainPort);
        _ports.Add(_parallelPort);
    }

    public override async ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        if (_parallelPort.Value)
        {
            if (_parallelTask != null)
            {
                await _parallelTask;
                _parallelTask.Dispose();
            }
            _parallelTask = Task.Run(async () => await Send(cancellationToken));
        }
        else
        {
            try
            {
                await Send(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while sending message to MQTT");
                return false;
            }
        }
        return true;
    }

    protected abstract ValueTask<bool> Send(CancellationToken cancellationToken);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (_parallelTask != null)
                {
                    _parallelTask.Wait(1000); // Wait for 1 second
                    _parallelTask.Dispose();
                }
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}