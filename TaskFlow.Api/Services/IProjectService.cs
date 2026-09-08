using TaskFlow.Api.DTOs.Projects;

namespace TaskFlow.Api.Services;

public interface IProjectService
{
    Task<List<ProjectResponse>> GetUserProjectsAsync(Guid userId);
    Task<ProjectResponse> GetProjectAsync(Guid id, Guid userId);
    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, Guid userId);
    Task<ProjectResponse> UpdateProjectAsync(Guid id, CreateProjectRequest request, Guid userId);
    Task DeleteProjectAsync(Guid id, Guid userId);
    Task<List<ProjectMemberResponse>> GetMembersAsync(Guid projectId, Guid userId);
    Task RemoveMemberAsync(Guid projectId, Guid memberUserId, Guid requesterId);
    Task LeaveProjectAsync(Guid projectId, Guid userId);
}