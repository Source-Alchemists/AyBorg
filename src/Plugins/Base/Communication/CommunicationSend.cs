using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public sealed class CommunicationSend : CommunicationSendBase
{
    private readonly StringPort _messageValuePort = new("Value", PortDirection.Input, string.Empty);

    public override string Name => "Communication.Send";

    public CommunicationSend(ILogger<CommunicationSendBase> logger, IDeviceManager deviceManager, ICommunicationStateProvider communicationStateProvider) : base(logger, deviceManager, communicationStateProvider)
    {
        _ports = _ports.Add(_messageValuePort);
    }

    protected override async ValueTask SendAsync(CancellationToken cancellationToken)
    {
        if(_device == null)
        {
            throw new InvalidOperationException("No device selected");
        }

        if(!await _device.TrySendAsync(_messageIdPort.Value, _messageValuePort))
        {
            throw new CommunicationException("Error while sending message to device");
        }
    }
}
