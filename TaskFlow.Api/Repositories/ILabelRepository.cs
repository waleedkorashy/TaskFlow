using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public interface ILabelRepository : IGenericRepository<Label>
{
    Task<List<Label>> GetByProjectIdAsync(Guid projectId);
    Task<Label?> GetByIdAndProjectAsync(Guid id, Guid projectId);
}