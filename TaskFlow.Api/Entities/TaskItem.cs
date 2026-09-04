namespace TaskFlow.Api.Entities;

/// <summary>
/// Represents a work item placed in a board column, optionally assigned to a user and tagged with labels.
/// </summary>
public class TaskItem
{
    public Guid Id { get; set; }
    public Guid BoardColumnId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime CreatedAt { get; set; }

    public BoardColumn BoardColumn { get; set; } = null!;
    public User? Assignee { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Label> Labels { get; set; } = new List<Label>();
}
