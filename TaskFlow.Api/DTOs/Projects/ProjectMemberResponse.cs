namespace TaskFlow.Api.DTOs.Projects;

public record ProjectMemberResponse(Guid UserId, string FullName, string Email, string Role);