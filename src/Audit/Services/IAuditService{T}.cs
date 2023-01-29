namespace AyBorg.Audit.Services;

public interface IAuditService<in T>
{
    bool TryAdd(T record);
}
