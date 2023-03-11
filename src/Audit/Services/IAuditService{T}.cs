using AyBorg.Data.Audit.Models;

namespace AyBorg.Audit.Services;

public interface IAuditService<in T>
{
    bool TryAdd(T record);
    bool TryRemove(Guid auditId);
    IEnumerable<ChangesetRecord> GetChangesets();
}
