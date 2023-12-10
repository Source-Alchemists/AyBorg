using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public sealed class CommunicationSend : CommunicationSendBase
{
    private readonly ILogger<CommunicationSend> _logger;
    private readonly StringPort _messageValuePort = new("Value", PortDirection.Input, string.Empty);

    public override string Name => "Communication.Send";

    public CommunicationSend(ILogger<CommunicationSend> logger, IDeviceManager deviceManager, ICommunicationStateProvider communicationStateProvider) : base(logger, deviceManager, communicationStateProvider)
    {
        _logger = logger;
        _ports = _ports.Add(_messageValuePort);
    }

    public override async ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        if (!_communicationStateProvider.IsResultCommunicationEnabled)
        {
            return true;
        }

        ChangeDevice();

        if (_parallelPort.Value)
        {
            if (_parallelTask != null)
            {
                await _parallelTask;
                _parallelTask.Dispose();
            }

            var clonedPort = new StringPort(_messageValuePort);
            _parallelTask = Task.Run(async () => await SendAsync(clonedPort), cancellationToken);
        }
        else
        {
            try
            {
                await SendAsync(_messageValuePort);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Result), ex, "Error while sending message to MQTT");
                return false;
            }
        }
        return true;
    }

    private async ValueTask SendAsync(StringPort port)
    {
        if (_device == null)
        {
            throw new InvalidOperationException("No device selected");
        }

        if (!await _device.TrySendAsync(_messageIdPort.Value, port))
        {
            throw new CommunicationException("Error while sending message to device");
        }
    }
}
