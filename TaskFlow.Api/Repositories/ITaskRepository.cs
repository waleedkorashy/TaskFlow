using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public interface ITaskRepository : IGenericRepository<TaskItem>
{
    Task<TaskItem?> GetWithAccessDataAsync(Guid id);
    Task<List<TaskItem>> GetByColumnIdOrderedAsync(Guid columnId);
}