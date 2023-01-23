using Ayborg.Gateway.Analytics.V1;
using AyBorg.SDK.Common;
using AyBorg.Web.Shared.Models;
using Grpc.Core;

namespace AyBorg.Web.Services.Analytics;

public sealed class EventLogService : IEventLogService
{
    private readonly ILogger<EventLogService> _logger;
    private readonly EventLog.EventLogClient _eventLogClient;


    public EventLogService(ILogger<EventLogService> logger, EventLog.EventLogClient eventLogClient)
    {
        _logger = logger;
        _eventLogClient = eventLogClient;
    }

    public async IAsyncEnumerable<EventLogEntry> GetEventsAsync()
    {
        AsyncServerStreamingCall<EventEntry> response = _eventLogClient.GetLogEvents(new GetEventsRequest
        {
            ServiceType = string.Empty,
            ServiceUniqueName = string.Empty,
            LogLevel = (int)LogLevel.None,
            EventId = -1
        });

        await foreach (EventEntry? entry in response.ResponseStream.ReadAllAsync())
        {
            yield return new EventLogEntry
            {
                ServiceType = entry.ServiceType,
                ServiceUniqueName = entry.ServiceUniqueName,
                Timestamp = entry.Timestamp.ToDateTime(),
                LogLevel = (LogLevel)entry.LogLevel,
                EventId = entry.EventId,
                EventName = GetEventTypeDescription(entry.EventId),
                Message = entry.Message
            };
        }
    }

    private static string GetEventTypeDescription(int id)
    {
        if (!Enum.IsDefined(typeof(EventLogType), id))
        {
            return "Undefined";
        }

        var eventLogType = (EventLogType)id;
        return $"{eventLogType.GetDescription()}";
    }
}
