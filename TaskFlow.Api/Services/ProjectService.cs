using TaskFlow.Api.Common.Exceptions;
using TaskFlow.Api.DTOs.Projects;
using TaskFlow.Api.Entities;
using TaskFlow.Api.Repositories;

namespace TaskFlow.Api.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    private static ProjectResponse ToResponse(Project p) => new(
        p.Id, p.Name, p.Description, p.OwnerId, p.Owner.FullName, p.CreatedAt, p.Members.Count
    );

    public async Task<List<ProjectResponse>> GetUserProjectsAsync(Guid userId)
    {
        var projects = await _projectRepository.GetUserProjectsAsync(userId);
        return projects.Select(ToResponse).ToList();
    }

    public async Task<ProjectResponse> GetProjectAsync(Guid id, Guid userId)
    {
        var project = await _projectRepository.GetWithDetailsAsync(id)
            ?? throw new NotFoundException("Project not found.");

        bool hasAccess = project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
        if (!hasAccess) throw new ForbiddenException("You do not have access to this project.");

        return ToResponse(project);
    }

    public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, Guid userId)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);

        var membership = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = userId,
            Role = "Admin"
        };
        await _projectRepository.AddMemberAsync(membership);

        await _projectRepository.SaveChangesAsync();

        var saved = await _projectRepository.GetWithDetailsAsync(project.Id);
        return ToResponse(saved!);
    }

    public async Task<ProjectResponse> UpdateProjectAsync(Guid id, CreateProjectRequest request, Guid userId)
    {
        var project = await _projectRepository.GetWithDetailsAsync(id)
            ?? throw new NotFoundException("Project not found.");

        if (project.OwnerId != userId)
            throw new ForbiddenException("Only the project owner can update this project.");

        project.Name = request.Name;
        project.Description = request.Description;

        await _projectRepository.SaveChangesAsync();
        return ToResponse(project);
    }

    public async Task DeleteProjectAsync(Guid id, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Project not found.");

        if (project.OwnerId != userId)
            throw new ForbiddenException("Only the project owner can delete this project.");

        _projectRepository.Remove(project);
        await _projectRepository.SaveChangesAsync();
    }

    public async Task<List<ProjectMemberResponse>> GetMembersAsync(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetWithMembersDetailAsync(projectId)
            ?? throw new NotFoundException("Project not found.");

        bool hasAccess = project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
        if (!hasAccess) throw new ForbiddenException("You do not have access to this project.");

        return project.Members
            .Select(m => new ProjectMemberResponse(m.UserId, m.User.FullName, m.User.Email!, m.Role))
            .ToList();
    }
    public async Task RemoveMemberAsync(Guid projectId, Guid memberUserId, Guid requesterId)
    {
        var project = await _projectRepository.GetWithDetailsAsync(projectId)
            ?? throw new NotFoundException("Project not found.");

        if (project.OwnerId != requesterId)
            throw new ForbiddenException("Only the project owner can remove members.");

        if (memberUserId == project.OwnerId)
            throw new ForbiddenException("The project owner cannot be removed.");

        var member = project.Members.FirstOrDefault(m => m.UserId == memberUserId)
            ?? throw new NotFoundException("This user is not a member of the project.");

        _projectRepository.RemoveMember(member);
        await _projectRepository.SaveChangesAsync();
    }

    public async Task LeaveProjectAsync(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetWithDetailsAsync(projectId)
            ?? throw new NotFoundException("Project not found.");

        if (project.OwnerId == userId)
            throw new ForbiddenException("The project owner cannot leave. Delete the project instead.");

        var member = project.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new NotFoundException("You are not a member of this project.");

        _projectRepository.RemoveMember(member);
        await _projectRepository.SaveChangesAsync();
    }
}