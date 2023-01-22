using Ayborg.Gateway.Analytics.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Analytics.Services;

public sealed class EventLogServiceV1 : EventLog.EventLogBase
{
    private readonly ILogger<EventLogServiceV1> _logger;

    public EventLogServiceV1(ILogger<EventLogServiceV1> logger)
    {
        _logger = logger;
    }

    public override Task<Empty> LogEvent(EventRequest request, ServerCallContext context)
    {
        _logger.LogInformation("{serviceType} ({serviceName}) {timestamp} - {logLevel} - {eventId} ({eventName}): {message}", request.ServiceType, request.ServiceUniqueName, request.TimeStamp.ToDateTime(), (LogLevel)request.LogLevel, request.EventId, request.EventName, request.Message);
        return Task.FromResult(new Empty());
    }
}
