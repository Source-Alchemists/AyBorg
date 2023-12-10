using Ayborg.Gateway.Analytics.V1;
using AyBorg.Data.Log;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AyBorg.Log.Services;

public sealed class EventLogServiceV1 : EventLog.EventLogBase
{
    private readonly ILogger<EventLogServiceV1> _logger;
    private readonly IEventStorage _eventStorage;

    public EventLogServiceV1(ILogger<EventLogServiceV1> logger, IEventStorage eventStorage)
    {
        _logger = logger;
        _eventStorage = eventStorage;
    }

    public override Task<Empty> LogEvent(EventEntry request, ServerCallContext context)
    {
        var entry = new EventRecord
        {
            ServiceType = request.ServiceType,
            ServiceUniqueName = request.ServiceUniqueName,
            Timestamp = request.Timestamp.ToDateTime(),
            LogLevel = (LogLevel)request.LogLevel,
            EventId = request.EventId,
            Message = request.Message
        };
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("{entry}", entry.ToString());
        }
        _eventStorage.Add(entry);
        return Task.FromResult(new Empty());
    }

    public override async Task GetLogEvents(GetEventsRequest request, IServerStreamWriter<EventEntry> responseStream, ServerCallContext context)
    {
        foreach (EventRecord entry in _eventStorage.GetRecords())
        {
            await responseStream.WriteAsync(new EventEntry
            {
                ServiceType = entry.ServiceType,
                ServiceUniqueName = entry.ServiceUniqueName,
                Timestamp = Timestamp.FromDateTime(entry.Timestamp),
                LogLevel = (int)entry.LogLevel,
                EventId = entry.EventId,
                Message = entry.Message
            });
        }
    }
}
