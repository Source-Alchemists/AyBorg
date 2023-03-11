using Ayborg.Gateway.Analytics.V1;
using AyBorg.Web.Shared.Mappers;
using AyBorg.Web.Shared.Models;
using Grpc.Core;

namespace AyBorg.Web.Services.Analytics;

public sealed class EventLogService : IEventLogService
{
    private readonly EventLog.EventLogClient _eventLogClient;


    public EventLogService(EventLog.EventLogClient eventLogClient)
    {
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
            yield return EventLogMapper.Map(entry);
        }
    }
}
