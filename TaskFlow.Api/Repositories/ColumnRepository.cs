using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public class ColumnRepository : GenericRepository<BoardColumn>, IColumnRepository
{
    public ColumnRepository(AppDbContext context) : base(context) { }

    public async Task<BoardColumn?> GetWithAccessDataAsync(Guid id)
    {
        return await DbSet
            .Include(c => c.Board).ThenInclude(b => b.Project).ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<BoardColumn?> GetWithTasksAndAccessDataAsync(Guid id)
    {
        return await DbSet
            .Include(c => c.Tasks)
            .Include(c => c.Board).ThenInclude(b => b.Project).ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}