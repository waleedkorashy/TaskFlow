using TaskFlow.Api.DTOs.Invitations;

namespace TaskFlow.Api.Services;

public interface IInvitationService
{
    Task<InvitationResponse> InviteAsync(Guid projectId, CreateInvitationRequest request, Guid inviterId);
    Task<InvitationPreviewResponse> GetPreviewAsync(string token);
    Task AcceptAsync(string token, Guid currentUserId, string currentUserEmail);
    Task<List<InvitationResponse>> GetPendingForProjectAsync(Guid projectId, Guid userId);
    Task RevokeAsync(Guid invitationId, Guid userId);
}