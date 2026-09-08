using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public interface ICommentRepository : IGenericRepository<Comment>
{
    Task<List<Comment>> GetByTaskIdAsync(Guid taskId);
    Task<Comment?> GetWithUserAsync(Guid id);
}