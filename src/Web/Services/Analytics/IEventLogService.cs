using AyBorg.Web.Shared.Models;

namespace AyBorg.Web.Services.Analytics;

public interface IEventLogService
{
    IAsyncEnumerable<EventLogEntry> GetEventsAsync();
}
