using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    public async Task<List<Project>> GetUserProjectsAsync(Guid userId)
    {
        return await DbSet
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    public async Task<Project?> GetWithDetailsAsync(Guid id)
    {
        return await DbSet
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> HasAccessAsync(Guid projectId, Guid userId)
    {
        return await DbSet.AnyAsync(p => p.Id == projectId &&
            (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));
    }

    public async Task AddMemberAsync(ProjectMember member)
    {
        await Context.ProjectMembers.AddAsync(member);
    }
    public async Task<Project?> GetWithMembersDetailAsync(Guid id)
    {
        return await DbSet
            .Include(p => p.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    public void RemoveMember(ProjectMember member)
    {
        Context.ProjectMembers.Remove(member);
    }
}