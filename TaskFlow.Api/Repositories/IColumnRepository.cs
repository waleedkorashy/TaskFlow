using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public interface IColumnRepository : IGenericRepository<BoardColumn>
{
    Task<BoardColumn?> GetWithAccessDataAsync(Guid id);
    Task<BoardColumn?> GetWithTasksAndAccessDataAsync(Guid id);
}