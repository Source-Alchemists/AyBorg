using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AyBorg.Data.Analytics;

public sealed class EventLogRepository : IEventLogRepository, IDisposable
{
    private readonly ILogger<EventLogRepository> _logger;
    private readonly LiteDatabase _database;
    private bool _disposedValue;

    public EventLogRepository(ILogger<EventLogRepository> logger, IConfiguration configuration)
    {
        _logger = logger;
        string connectionString = configuration.GetConnectionString("Database")!;
        _database = new LiteDatabase(connectionString)
        {
            UtcDate = true
        };
    }

    public bool TryAdd(EventRecord eventRecord)
    {
        try
        {
            ILiteCollection<EventRecord> collection = _database.GetCollection<EventRecord>("events");
            collection.Insert(eventRecord);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add event record.");
            return false;
        }
    }

    public bool TryRemove(EventRecord eventRecord)
    {
        try
        {
            ILiteCollection<EventRecord> collection = _database.GetCollection<EventRecord>("events");
            collection.DeleteMany(e => e.Id.Equals(eventRecord.Id));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove event record.");
            return false;
        }
    }

    public bool TryRemove(IEnumerable<EventRecord> eventRecords)
    {
        try
        {
            ILiteCollection<EventRecord> collection = _database.GetCollection<EventRecord>("events");
            foreach (EventRecord outdatedEvent in eventRecords)
            {
                collection.Delete(outdatedEvent.Id);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove event records.");
            return false;
        }
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
