namespace AyBorg.Data.Audit;

public interface IAuditRepository<T>
{
    bool TryAdd(T record);
    bool TryRemove(T record);
    T Find(Guid auditId);
    IEnumerable<T> FindAll();
    IEnumerable<T> FindAll(DateTime from, DateTime to);
}
