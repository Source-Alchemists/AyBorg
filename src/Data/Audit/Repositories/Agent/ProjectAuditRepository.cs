using AyBorg.Data.Audit.Models.Agent;
using AyBorg.SDK.Common;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AyBorg.Data.Audit.Repositories.Agent;

public sealed class AgentProjectAuditRepository : IProjectAuditRepository
{
    private readonly ILogger<AgentProjectAuditRepository> _logger;
    private readonly string _connectionString;

    public AgentProjectAuditRepository(ILogger<AgentProjectAuditRepository> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("Database")!;
    }

    public bool TryAdd(ProjectAuditRecord record)
    {
        try
        {
            using LiteDatabase database = CreateDatabase();
            ILiteCollection<ProjectAuditRecord> collection = database.GetCollection<ProjectAuditRecord>("agentProjectAudits");
            collection.Insert(record);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to add audit record.");
            return false;
        }
    }
    public bool TryRemove(ProjectAuditRecord record)
    {
        try
        {
            using LiteDatabase database = CreateDatabase();
            ILiteCollection<ProjectAuditRecord> collection = database.GetCollection<ProjectAuditRecord>("agentProjectAudits");
            collection.DeleteMany(e => e.Id.Equals(record.Id));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to remove audit record.");
            return false;
        }
    }

    public ProjectAuditRecord Find(Guid auditId)
    {
        using LiteDatabase database = CreateDatabase();
        ILiteCollection<ProjectAuditRecord> collection = database.GetCollection<ProjectAuditRecord>("agentProjectAudits");
        return collection.FindOne(a => a.Id.Equals(auditId));
    }

    public IEnumerable<ProjectAuditRecord> FindAll(DateTime from, DateTime to)
    {
        using LiteDatabase database = CreateDatabase();
        ILiteCollection<ProjectAuditRecord> collection = database.GetCollection<ProjectAuditRecord>("agentProjectAudits");
        return collection.Find(a => a.Timestamp >= from && a.Timestamp <= to).ToList();
    }

    private LiteDatabase CreateDatabase()
    {
        return new LiteDatabase(_connectionString)
        {
            UtcDate = true
        };
    }
}
