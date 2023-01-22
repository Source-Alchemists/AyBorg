using AyBorg.Data.Analytics;

namespace AyBorg.Web.Services.Analytics;

public interface IEventLogService
{
    IAsyncEnumerable<EventRecord> GetEventsAsync();
}
