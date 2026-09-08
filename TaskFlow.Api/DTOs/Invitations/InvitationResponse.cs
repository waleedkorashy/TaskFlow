namespace TaskFlow.Api.DTOs.Invitations;

public record InvitationResponse(
    Guid Id, Guid ProjectId, string ProjectName, string Email,
    string Role, string Status, DateTime CreatedAt, DateTime ExpiresAt
);