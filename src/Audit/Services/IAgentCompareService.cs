using AyBorg.Data.Audit.Models;
using AyBorg.Data.Audit.Models.Agent;

namespace AyBorg.Audit.Services;

public interface IAgentCompareService
{
    IEnumerable<ChangeRecord> Compare(IEnumerable<ProjectAuditRecord> projectAuditRecords);
}
