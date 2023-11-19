using AyBorg.Data.Log;

namespace AyBorg.Log.Services;

public interface IEventStorage
{
    void Add(EventRecord eventRecord);
    IEnumerable<EventRecord> GetRecords();
}
