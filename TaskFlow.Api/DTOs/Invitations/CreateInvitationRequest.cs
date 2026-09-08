namespace TaskFlow.Api.DTOs.Invitations;

public record CreateInvitationRequest(string Email, string Role = "Member");