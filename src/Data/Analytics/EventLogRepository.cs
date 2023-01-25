using LiteDB;
using Microsoft.Extensions.Configuration;

namespace AyBorg.Data.Analytics;

public sealed class EventLogRepository : IEventLogRepository, IDisposable
{
    private readonly LiteDatabase _database;
    private bool _disposedValue;

    public EventLogRepository(IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DatabaseConnection");
        _database = new LiteDatabase(connectionString)
        {
            UtcDate = true
        };
    }

    public bool TryAdd(EventRecord eventRecord)
    {
        ILiteCollection<EventRecord> collection = _database.GetCollection<EventRecord>("events");
        collection.Insert(eventRecord with { Timestamp = DateTime.SpecifyKind(eventRecord.Timestamp, DateTimeKind.Utc) });
        return _database.Commit();
    }

    public bool TryDelete(EventRecord eventRecord)
    {
        ILiteCollection<EventRecord> collection = _database.GetCollection<EventRecord>("events");
        collection.DeleteMany(e => e.Id.Equals(eventRecord.Id));
        return _database.Commit();
    }

    public bool TryDelete(IEnumerable<EventRecord> eventRecords)
    {
        ILiteCollection<EventRecord> collection = _database.GetCollection<EventRecord>("events");
        foreach(EventRecord outdatedEvent in eventRecords)
        {
            collection.Delete(outdatedEvent.Id);
        }
        return _database.Commit();
    }

    public IEnumerable<EventRecord> FindAll()
    {
        ILiteCollection<EventRecord> collection = _database.GetCollection<EventRecord>("events");
        return collection.FindAll().ToList();
    }

    public IEnumerable<EventRecord> FindAll(DateTime from, DateTime to)
    {
        ILiteCollection<EventRecord> collection = _database.GetCollection<EventRecord>("events");
        return collection.Find(d => d.Timestamp >= from && d.Timestamp <= to).ToList();
    }

    public IEnumerable<EventRecord> FindAllTill(DateTime target)
    {
        ILiteCollection<EventRecord> collection = _database.GetCollection<EventRecord>("events");
        return collection.Find(d => d.Timestamp <= target).ToList();
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            _database?.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
