namespace AyBorg.Data.Audit;

public interface IAuditRepository<T>
{
    bool TryAdd(T record);
    bool TryRemove(T record);
    IEnumerable<T> FindAll();
}
