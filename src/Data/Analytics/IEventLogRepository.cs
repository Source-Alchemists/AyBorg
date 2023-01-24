namespace AyBorg.Data.Analytics;

public interface IEventLogRepository
{
    bool TryAdd(EventRecord eventRecord);
    bool TryDelete(EventRecord eventRecord);
    bool TryDelete(IEnumerable<EventRecord> eventRecords);
    IEnumerable<EventRecord> FindAll();
    IEnumerable<EventRecord> FindAll(DateTime from, DateTime to);
    IEnumerable<EventRecord> FindAllTill(DateTime target);
}
