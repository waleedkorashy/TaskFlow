using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
{
    public TaskRepository(AppDbContext context) : base(context) { }

    public async Task<TaskItem?> GetWithAccessDataAsync(Guid id)
    {
        return await DbSet
            .Include(t => t.Assignee)
            .Include(t => t.Labels)
            .Include(t => t.BoardColumn).ThenInclude(c => c.Board).ThenInclude(b => b.Project).ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TaskItem>> GetByColumnIdOrderedAsync(Guid columnId)
    {
        return await DbSet
            .Include(t => t.Assignee)
            .Where(t => t.BoardColumnId == columnId)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }
}