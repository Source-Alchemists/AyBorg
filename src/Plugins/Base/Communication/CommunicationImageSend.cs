using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public sealed class CommunicationImageSend : CommunicationSendBase
{
    private readonly ILogger<CommunicationImageSend> _logger;
    private readonly ImagePort _messageValuePort = new("Value", PortDirection.Input, null!);
    private ImagePort? _imagePortClone;
    private bool _isDisposed = false;

    public CommunicationImageSend(ILogger<CommunicationImageSend> logger, IDeviceManager deviceManager, ICommunicationStateProvider communicationStateProvider) : base(logger, deviceManager, communicationStateProvider)
    {
        _logger = logger;
        _ports = _ports.Add(_messageValuePort);
    }

    public override string Name => "Communication.Image.Send";

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

            _imagePortClone?.Dispose();
            _imagePortClone = new ImagePort(_messageValuePort);
            _parallelTask = Task.Run(async () => await SendAsync(_imagePortClone), cancellationToken);
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

    private async ValueTask SendAsync(ImagePort port)
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

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        if (isDisposing && !_isDisposed)
        {
            _imagePortClone?.Dispose();
            _isDisposed = true;
        }
    }
}
