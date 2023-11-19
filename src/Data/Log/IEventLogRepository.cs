namespace AyBorg.Data.Log;

public interface IEventLogRepository
{
    bool TryAdd(EventRecord eventRecord);
    bool TryRemove(EventRecord eventRecord);
    bool TryRemove(IEnumerable<EventRecord> eventRecords);
    IEnumerable<EventRecord> FindAll();
    IEnumerable<EventRecord> FindAll(DateTime from, DateTime to);
    IEnumerable<EventRecord> FindAllTill(DateTime target);
}
