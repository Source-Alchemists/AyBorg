using AyBorg.Data.Audit.Models.Agent;

namespace AyBorg.Data.Audit.Repositories.Agent;

public interface IProjectAuditRepository : IAuditRepository<ProjectAuditRecord>
{
    IEnumerable<ProjectAuditRecord> FindAll(DateTime from, DateTime to);
    ProjectAuditRecord Find(Guid auditId);
}
