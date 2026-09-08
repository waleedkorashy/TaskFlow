using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public interface IInvitationRepository : IGenericRepository<ProjectInvitation>
{
    Task<ProjectInvitation?> GetByTokenAsync(string token);
    Task<ProjectInvitation?> GetPendingByProjectAndEmailAsync(Guid projectId, string email);
    Task<List<ProjectInvitation>> GetPendingByProjectAsync(Guid projectId);
}