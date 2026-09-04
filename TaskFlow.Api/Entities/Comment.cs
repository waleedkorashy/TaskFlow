namespace TaskFlow.Api.Entities;

/// <summary>
/// Represents a user comment attached to a task.
/// </summary>
public class Comment
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public TaskItem TaskItem { get; set; } = null!;
    public User User { get; set; } = null!;
}
