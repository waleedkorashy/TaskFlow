using TaskFlow.Api.Common.Exceptions;
using TaskFlow.Api.DTOs.Invitations;
using TaskFlow.Api.Entities;
using TaskFlow.Api.Repositories;

namespace TaskFlow.Api.Services;

public class InvitationService : IInvitationService
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public InvitationService(
        IInvitationRepository invitationRepository,
        IProjectRepository projectRepository,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _invitationRepository = invitationRepository;
        _projectRepository = projectRepository;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    private static InvitationResponse ToResponse(ProjectInvitation i, string projectName) => new(
        i.Id, i.ProjectId, projectName, i.Email, i.Role, i.Status, i.CreatedAt, i.ExpiresAt
    );

    private static string GenerateToken() => Guid.NewGuid().ToString("N");

    private async Task SendInviteEmail(ProjectInvitation invitation, string projectName, string inviterName)
    {
        var baseUrl = _configuration["Frontend:BaseUrl"];
        var acceptUrl = $"{baseUrl}/invitations/{invitation.Token}";
        await _emailSender.SendProjectInvitationEmailAsync(invitation.Email, projectName, inviterName, acceptUrl);
    }

    public async Task<InvitationResponse> InviteAsync(Guid projectId, CreateInvitationRequest request, Guid inviterId)
    {
        var project = await _projectRepository.GetWithDetailsAsync(projectId)
            ?? throw new NotFoundException("Project not found.");

        if (project.OwnerId != inviterId)
            throw new ForbiddenException("Only the project owner can invite members.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (project.Members.Any(m => m.User != null && m.User.Email!.ToLower() == normalizedEmail))
            throw new ConflictException("This person is already a member of the project.");

        var existing = await _invitationRepository.GetPendingByProjectAndEmailAsync(projectId, normalizedEmail);
        if (existing != null)
        {
            existing.Token = GenerateToken();
            existing.ExpiresAt = DateTime.UtcNow.AddDays(7);
            await _invitationRepository.SaveChangesAsync();
            await SendInviteEmail(existing, project.Name, project.Owner.FullName);
            return ToResponse(existing, project.Name);
        }

        var invitation = new ProjectInvitation
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Email = normalizedEmail,
            Role = string.IsNullOrWhiteSpace(request.Role) ? "Member" : request.Role,
            Token = GenerateToken(),
            Status = "Pending",
            InvitedByUserId = inviterId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _invitationRepository.AddAsync(invitation);
        await _invitationRepository.SaveChangesAsync();

        await SendInviteEmail(invitation, project.Name, project.Owner.FullName);

        return ToResponse(invitation, project.Name);
    }

    public async Task<InvitationPreviewResponse> GetPreviewAsync(string token)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(token)
            ?? throw new NotFoundException("Invitation not found.");

        bool isValid = invitation.Status == "Pending" && invitation.ExpiresAt > DateTime.UtcNow;

        return new InvitationPreviewResponse(invitation.Project.Name, invitation.InvitedBy.FullName, invitation.Email, isValid);
    }

    public async Task AcceptAsync(string token, Guid currentUserId, string currentUserEmail)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(token)
            ?? throw new NotFoundException("Invitation not found.");

        if (invitation.Status != "Pending")
            throw new ConflictException("This invitation is no longer valid.");

        if (invitation.ExpiresAt < DateTime.UtcNow)
            throw new ConflictException("This invitation has expired.");

        if (!string.Equals(invitation.Email, currentUserEmail, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("This invitation was sent to a different email address.");

        var project = await _projectRepository.GetWithDetailsAsync(invitation.ProjectId)
            ?? throw new NotFoundException("Project not found.");

        bool alreadyMember = project.OwnerId == currentUserId || project.Members.Any(m => m.UserId == currentUserId);
        if (!alreadyMember)
        {
            await _projectRepository.AddMemberAsync(new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                UserId = currentUserId,
                Role = invitation.Role
            });
        }

        invitation.Status = "Accepted";
        await _invitationRepository.SaveChangesAsync();
    }

    public async Task<List<InvitationResponse>> GetPendingForProjectAsync(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new NotFoundException("Project not found.");

        if (project.OwnerId != userId)
            throw new ForbiddenException("Only the project owner can view invitations.");

        var invitations = await _invitationRepository.GetPendingByProjectAsync(projectId);
        return invitations.Select(i => ToResponse(i, project.Name)).ToList();
    }

    public async Task RevokeAsync(Guid invitationId, Guid userId)
    {
        var invitation = await _invitationRepository.GetByIdAsync(invitationId)
            ?? throw new NotFoundException("Invitation not found.");

        var project = await _projectRepository.GetByIdAsync(invitation.ProjectId)
            ?? throw new NotFoundException("Project not found.");

        if (project.OwnerId != userId)
            throw new ForbiddenException("Only the project owner can revoke invitations.");

        _invitationRepository.Remove(invitation);
        await _invitationRepository.SaveChangesAsync();
    }
}