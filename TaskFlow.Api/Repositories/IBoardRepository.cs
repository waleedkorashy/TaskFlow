using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public interface IBoardRepository : IGenericRepository<Board>
{
    Task<List<Board>> GetByProjectIdAsync(Guid projectId);
    Task<Board?> GetWithDetailsAsync(Guid id);
}