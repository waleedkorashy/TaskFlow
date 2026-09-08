using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public class BoardRepository : GenericRepository<Board>, IBoardRepository
{
    public BoardRepository(AppDbContext context) : base(context) { }

    public async Task<List<Board>> GetByProjectIdAsync(Guid projectId)
    {
        return await DbSet
            .Include(b => b.BoardColumns)
            .Where(b => b.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<Board?> GetWithDetailsAsync(Guid id)
    {
        return await DbSet
            .Include(b => b.BoardColumns)
            .Include(b => b.Project).ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}