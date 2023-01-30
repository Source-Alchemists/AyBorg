using AyBorg.Data.Audit.Models.Agent;
using AyBorg.Data.Audit.Repositories.Agent;
using AyBorg.SDK.Common;

namespace AyBorg.Audit.Services;

public sealed class AgentAuditService : IAgentAuditService
{
    private readonly ILogger<AgentAuditService> _logger;
    private readonly IProjectAuditRepository _projectAuditRepository;

    public AgentAuditService(ILogger<AgentAuditService> logger, IProjectAuditRepository projectAuditRepository)
    {
        _logger = logger;
        _projectAuditRepository = projectAuditRepository;
    }

    public bool TryAdd(ProjectAuditRecord record)
    {
        bool result = _projectAuditRepository.TryAdd(record);
        if (result == true)
        {
            _logger.LogInformation(new EventId((int)EventLogType.Audit), "Audit entry added for project [{projectName}] with state [{projectState}].", record.Name, record.State);
        }
        return result;
    }

    public bool TryRemove(Guid auditId)
    {
        try
        {
            ProjectAuditRecord record = _projectAuditRepository.Find(auditId);
            if (record == null)
            {
                throw new AuditException("Failed to find audit record.");
            }
            _logger.LogInformation(new EventId((int)EventLogType.Audit), "Audit entry removed for project [{projectName}] with state [{projectState}].", record.Name, record.State);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Audit), ex, "Failed to remove audit entry.");
            return false;
        }
    }
}
