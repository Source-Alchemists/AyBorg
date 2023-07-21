using AyBorg.SDK.Common;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Communication;

public class CommunicationSend : CommunicationBase
{
    private readonly ILogger<CommunicationSend> _logger;
    public override string Name => "Communication.Send";

    public CommunicationSend(ILogger<CommunicationSend> logger, IDeviceManager deviceManager) : base(deviceManager)
    {
        _logger = logger;
    }

    public override ValueTask<bool> TryRunAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}
