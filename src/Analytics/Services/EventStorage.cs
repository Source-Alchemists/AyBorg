using System.Collections.Concurrent;
using AyBorg.Data.Analytics;

namespace AyBorg.Analytics.Services;

public sealed class EventStorage : IEventStorage
{
    private readonly ConcurrentQueue<EventRecord> _queue = new();
    private readonly int _maxMemoryEntries = 1000;

    public EventStorage(IConfiguration configuration)
    {
        _maxMemoryEntries = configuration.GetValue("AyBorg:EventStorage:MaxInMemoryEntries", 1000);
    }

    public void Add(EventRecord eventRecord)
    {
        if(_queue.Count > _maxMemoryEntries)
        {
            _queue.TryDequeue(out EventRecord? _);
        }

        _queue.Enqueue(eventRecord);
    }

    public IEnumerable<EventRecord> GetRecords()
    {
        return _queue.AsEnumerable();
    }
}
