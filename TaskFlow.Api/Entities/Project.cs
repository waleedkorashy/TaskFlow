namespace TaskFlow.Api.Entities;

/// <summary>
/// Represents a workspace that groups boards, members, and labels for collaborative task management.
/// </summary>
public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Label> Labels { get; set; } = new List<Label>();
}
