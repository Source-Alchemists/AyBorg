using AyBorg.Data.Analytics;

namespace AyBorg.Analytics.Services;

public interface IEventStorage
{
    void Add(EventRecord eventRecord);
    IEnumerable<EventRecord> GetRecords();
}
