namespace AyBorg.Hub.Database;

public interface IRepository<T>
{
    ValueTask<IQueryable<T>> GetAsync(string? id);
    ValueTask<T> AddAsync(T entity);
    ValueTask<T> UpdateAsync(T entity);
    ValueTask<T?> DeleteAsync(string id);
}
