using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(AppDbContext context) : base(context) { }

    public async Task<List<Comment>> GetByTaskIdAsync(Guid taskId)
    {
        return await DbSet
            .Include(c => c.User)
            .Where(c => c.TaskItemId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment?> GetWithUserAsync(Guid id)
    {
        return await DbSet.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
    }
}