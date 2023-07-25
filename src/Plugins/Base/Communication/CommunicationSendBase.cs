using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public abstract class CommunicationSendBase : CommunicationBase
{
    private readonly ILogger<CommunicationSendBase> _logger;
    private readonly ICommunicationStateProvider _communicationStateProvider;
    private Task _parallelTask = null!;
    private bool _isDisposed = false;
    protected readonly BooleanPort _parallelPort = new("Parallel", PortDirection.Input, false);

    protected CommunicationSendBase(ILogger<CommunicationSendBase> logger, IDeviceManager deviceManager, ICommunicationStateProvider communicationStateProvider) : base(logger, deviceManager)
    {
        _logger = logger;
        _communicationStateProvider = communicationStateProvider;
        _ports = _ports.Add(_parallelPort);
        _ports = _ports.Add(_messageIdPort);
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

            _parallelTask = Task.Run(async () => await SendAsync(cancellationToken), cancellationToken);
        }
        else
        {
            try
            {
                await SendAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(new EventId((int)EventLogType.Result), ex, "Error while sending message to MQTT");
                return false;
            }
        }
        return true;
    }

    protected abstract ValueTask SendAsync(CancellationToken cancellationToken);

    protected override void Dispose(bool isDisposing)
    {
        if(isDisposing && !_isDisposed)
        {
            if (_parallelTask != null)
            {
                _parallelTask.Wait(1000); // Wait for 1 second
                _parallelTask.Dispose();
            }

            _isDisposed = true;
        }

        base.Dispose(isDisposing);
    }
}
