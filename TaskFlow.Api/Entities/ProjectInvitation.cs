namespace TaskFlow.Api.Entities;

public class ProjectInvitation
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = "Member";
    public string Token { get; set; } = null!;
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Revoked
    public Guid InvitedByUserId { get; set; }
    public User InvitedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}