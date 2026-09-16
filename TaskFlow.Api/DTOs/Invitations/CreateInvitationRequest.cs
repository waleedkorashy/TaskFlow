using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Invitations;

public record CreateInvitationRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    string Role = "Member");