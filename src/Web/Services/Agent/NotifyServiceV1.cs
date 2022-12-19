using Ayborg.Gateway.Agent.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Web.Services.Agent;

public sealed class NotifyServiceV1 : Notify.NotifyBase
{
    private readonly ILogger<NotifyServiceV1> _logger;
    private readonly INotifyService _notifyService;

    public NotifyServiceV1(ILogger<NotifyServiceV1> logger, INotifyService notifyService)
    {
        _logger = logger;
        _notifyService = notifyService;
    }

    public override Task<Empty> EngineIterationFinished(EngineIterationFinishedArgsDto request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.IterationId, out Guid iterationId))
        {
            _notifyService.AgentIterationFinished?.Invoke(iterationId);
        }

        return Task.FromResult(new Empty());
    }
}
