using AyBorg.Data.Audit.Models.Agent;
using AyBorg.Data.Audit.Repositories.Agent;

namespace AyBorg.Audit.Services;

public sealed class AgentAuditService : IAgentAuditService
{
    private readonly IProjectAuditRepository _projectAuditRepository;

    public AgentAuditService(IProjectAuditRepository projectAuditRepository)
    {
        _projectAuditRepository = projectAuditRepository;
    }

    public bool TryAdd(ProjectAuditRecord record) => _projectAuditRepository.TryAdd(record);
}
