namespace AyBorg.Data.Audit;

public interface IAuditRepository<T>
{
    bool TryAdd(T entry);
}
