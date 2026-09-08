namespace TaskFlow.Api.DTOs.Projects;

public record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    string OwnerName,
    DateTime CreatedAt,
    int MemberCount
);