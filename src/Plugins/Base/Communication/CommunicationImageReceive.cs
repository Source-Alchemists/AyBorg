using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Communication;
using AyBorg.SDK.Common.Ports;
using ImageTorque;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace AyBorg.Plugins.Base.Communication;

public sealed class CommunicationImageReceive : CommunicationReceiveBase
{
    private readonly ILogger<CommunicationImageReceive> _logger;
    private readonly ImagePort _valuePort = new("Value", PortDirection.Output, null!);
    private readonly RecyclableMemoryStreamManager _memoryStreamManager = new();
    public override string Name => "Communication.Image.Receive";

    public CommunicationImageReceive(ILogger<CommunicationImageReceive> logger, IDeviceManager deviceManager) : base(logger, deviceManager)
    {
        _logger = logger;
        _ports = _ports.Add(_valuePort);
    }

    protected override void OnMessageReceived(object? sender, MessageEventArgs e)
    {
        if (e.Message.Payload == ArraySegment<byte>.Empty)
        {
            _logger.LogWarning(new EventId((int)EventLogType.Plugin), "Received message with empty payload");
            _valuePort.Value = null!;
        }
        else
        {
            using MemoryStream stream = _memoryStreamManager.GetStream(e.Message.Payload);
            var image = Image.Load(stream);
            _valuePort.Value?.Dispose();
            _valuePort.Value = image;
        }

        _hasNewMessage = true;
    }
}
