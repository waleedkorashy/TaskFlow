using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public class InvitationRepository : GenericRepository<ProjectInvitation>, IInvitationRepository
{
    public InvitationRepository(AppDbContext context) : base(context) { }

    public async Task<ProjectInvitation?> GetByTokenAsync(string token)
    {
        return await DbSet
            .Include(i => i.Project)
            .Include(i => i.InvitedBy)
            .FirstOrDefaultAsync(i => i.Token == token);
    }

    public async Task<ProjectInvitation?> GetPendingByProjectAndEmailAsync(Guid projectId, string email)
    {
        return await DbSet.FirstOrDefaultAsync(i =>
            i.ProjectId == projectId && i.Email == email && i.Status == "Pending");
    }

    public async Task<List<ProjectInvitation>> GetPendingByProjectAsync(Guid projectId)
    {
        return await DbSet
            .Where(i => i.ProjectId == projectId && i.Status == "Pending")
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }
}