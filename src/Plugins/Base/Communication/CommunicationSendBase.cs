using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.System.Runtime;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public abstract class CommunicationSendBase : CommunicationBase
{
    private bool _isDisposed = false;
    protected readonly ICommunicationStateProvider _communicationStateProvider;
    protected Task _parallelTask = null!;
    protected readonly BooleanPort _parallelPort = new("Parallel", PortDirection.Input, false);

    protected CommunicationSendBase(ILogger<CommunicationSendBase> logger, IDeviceManager deviceManager, ICommunicationStateProvider communicationStateProvider) : base(logger, deviceManager)
    {
        _communicationStateProvider = communicationStateProvider;
        _ports = _ports.Add(_parallelPort);
        _ports = _ports.Add(_messageIdPort);
    }

    public abstract override ValueTask<bool> TryRunAsync(CancellationToken cancellationToken);

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
