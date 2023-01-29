namespace AyBorg.Audit.Services;

public interface IAuditService<T>
{
    bool TryAdd(T record);
}
