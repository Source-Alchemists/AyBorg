using System.Text;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Communication;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public sealed class CommunicationReceive : CommunicationReceiveBase
{
    private readonly ILogger<CommunicationReceive> _logger;
    private readonly StringPort _messagePort = new("Message", PortDirection.Output, string.Empty);
    public override string Name => "Communication.Receive";

    public CommunicationReceive(ILogger<CommunicationReceive> logger, IDeviceManager deviceManager) : base(logger, deviceManager)
    {
        _logger = logger;
        _ports = _ports.Add(_messagePort);
    }

    protected override void OnMessageReceived(object? sender, MessageEventArgs e)
    {
        if (e.Message.Payload == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Received message with null payload");
            return;
        }

        _messagePort.Value = Encoding.UTF8.GetString(e.Message.Payload);
        _hasNewMessage = true;
    }
}
