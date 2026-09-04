namespace TaskFlow.Api.Entities;

/// <summary>
/// Represents a user's membership and role within a project.
/// </summary>
public class ProjectMember
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}
