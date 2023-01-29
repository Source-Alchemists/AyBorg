using AyBorg.Data.Audit.Models.Agent;

namespace AyBorg.Data.Audit.Repositories.Agent;

public sealed class AgentProjectAuditRepository : IProjectAuditRepository
{
    public bool TryAdd(ProjectAuditRecord entry) => throw new NotImplementedException();
}
