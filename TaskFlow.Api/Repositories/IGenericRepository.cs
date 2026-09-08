namespace TaskFlow.Api.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();
}