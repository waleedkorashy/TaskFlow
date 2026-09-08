using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public class LabelRepository : GenericRepository<Label>, ILabelRepository
{
    public LabelRepository(AppDbContext context) : base(context) { }

    public async Task<List<Label>> GetByProjectIdAsync(Guid projectId)
    {
        return await DbSet.Where(l => l.ProjectId == projectId).ToListAsync();
    }

    public async Task<Label?> GetByIdAndProjectAsync(Guid id, Guid projectId)
    {
        return await DbSet.FirstOrDefaultAsync(l => l.Id == id && l.ProjectId == projectId);
    }
}