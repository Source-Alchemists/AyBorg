namespace AyBorg.Data.Audit;

public interface IAuditRepository<in T>
{
    bool TryAdd(T entry);
}
