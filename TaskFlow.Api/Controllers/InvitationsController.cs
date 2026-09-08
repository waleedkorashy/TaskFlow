using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs.Invitations;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[ApiController]
public class InvitationsController : ControllerBase
{
    private readonly IInvitationService _invitationService;
    public InvitationsController(IInvitationService invitationService) => _invitationService = invitationService;

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetCurrentUserEmail() => User.FindFirstValue(ClaimTypes.Email)!;

    [Authorize]
    [HttpPost("api/projects/{projectId}/invitations")]
    public async Task<ActionResult<InvitationResponse>> Invite(Guid projectId, CreateInvitationRequest request)
    {
        return Ok(await _invitationService.InviteAsync(projectId, request, GetCurrentUserId()));
    }

    [Authorize]
    [HttpGet("api/projects/{projectId}/invitations")]
    public async Task<ActionResult<List<InvitationResponse>>> GetPending(Guid projectId)
    {
        return Ok(await _invitationService.GetPendingForProjectAsync(projectId, GetCurrentUserId()));
    }

    [Authorize]
    [HttpDelete("api/invitations/{id}")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        await _invitationService.RevokeAsync(id, GetCurrentUserId());
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("api/invitations/{token}")]
    public async Task<ActionResult<InvitationPreviewResponse>> Preview(string token)
    {
        return Ok(await _invitationService.GetPreviewAsync(token));
    }

    [Authorize]
    [HttpPost("api/invitations/{token}/accept")]
    public async Task<IActionResult> Accept(string token)
    {
        await _invitationService.AcceptAsync(token, GetCurrentUserId(), GetCurrentUserEmail());
        return Ok(new { message = "You have joined the project." });
    }
}