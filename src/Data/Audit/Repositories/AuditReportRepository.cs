using AyBorg.Data.Audit.Models;
using AyBorg.SDK.Common;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AyBorg.Data.Audit.Repositories;

public sealed class AuditReportRepository : IAuditReportRepository
{
    private readonly ILogger<AuditReportRepository> _logger;
    private readonly string _connectionString;

    public AuditReportRepository(ILogger<AuditReportRepository> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("Database")!;
    }

    public bool TryAdd(AuditReportRecord record)
    {
        try
        {
            using LiteDatabase database = CreateDatabase();
            ILiteCollection<AuditReportRecord> collection = database.GetCollection<AuditReportRecord>("auditReports");
            collection.Insert(record);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to add audit record.");
            return false;
        }
    }

    public bool TryRemove(AuditReportRecord record)
    {
        try
        {
            using LiteDatabase database = CreateDatabase();
            ILiteCollection<AuditReportRecord> collection = database.GetCollection<AuditReportRecord>("auditReports");
            collection.DeleteMany(e => e.Id.Equals(record.Id));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to remove audit record.");
            return false;
        }
    }
    public IEnumerable<AuditReportRecord> FindAll()
    {
        using LiteDatabase database = CreateDatabase();
        ILiteCollection<AuditReportRecord> collection = database.GetCollection<AuditReportRecord>("auditReports");
        return collection.FindAll().ToList();
    }

    private LiteDatabase CreateDatabase()
    {
        return new LiteDatabase(_connectionString)
        {
            UtcDate = true
        };
    }
}
