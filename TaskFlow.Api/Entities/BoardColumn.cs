namespace TaskFlow.Api.Entities;

/// <summary>
/// Represents a column on a Kanban board that holds ordered tasks.
/// </summary>
public class BoardColumn
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Board Board { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
