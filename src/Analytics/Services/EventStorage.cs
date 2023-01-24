using AyBorg.Data.Analytics;

namespace AyBorg.Analytics.Services;

public sealed class EventStorage : IEventStorage
{
    private readonly IEventLogRepository _eventLogRepository;
    private readonly int _maxDaysToKeep;

    public EventStorage(IConfiguration configuration, IEventLogRepository eventLogRepository)
    {
        _eventLogRepository = eventLogRepository;
        _maxDaysToKeep = configuration.GetValue("AyBorg:EventStorage:MaxDaysToKeep", 30);
    }

    public void Add(EventRecord eventRecord)
    {
        IEnumerable<EventRecord> outdatedEvents = _eventLogRepository.FindAllTill(DateTime.UtcNow - TimeSpan.FromDays(_maxDaysToKeep));
        _eventLogRepository.TryDelete(outdatedEvents);
        _eventLogRepository.TryAdd(eventRecord);
    }

    public IEnumerable<EventRecord> GetRecords()
    {
        return _eventLogRepository.FindAll().OrderBy(e => e.Timestamp);
    }
}
