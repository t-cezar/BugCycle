namespace BugCycle.Application.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<List<T>> ListAllAsync(CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);

    void Update(T entity);

    void Remove(T entity);

    void RemoveRange(IEnumerable<T> entities);
}
