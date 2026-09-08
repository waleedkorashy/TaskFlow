using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Repositories;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<List<Project>> GetUserProjectsAsync(Guid userId);
    Task<Project?> GetWithDetailsAsync(Guid id);
    Task<bool> HasAccessAsync(Guid projectId, Guid userId);
    Task AddMemberAsync(ProjectMember member);
    Task<Project?> GetWithMembersDetailAsync(Guid id);
    void RemoveMember(ProjectMember member);
}
