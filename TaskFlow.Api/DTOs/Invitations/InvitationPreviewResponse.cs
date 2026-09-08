namespace TaskFlow.Api.DTOs.Invitations;

public record InvitationPreviewResponse(string ProjectName, string InviterName, string Email, bool IsValid);