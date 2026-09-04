namespace TaskFlow.Api.Entities;

/// <summary>
/// Represents a Kanban board belonging to a project.
/// </summary>
public class Board
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<BoardColumn> BoardColumns { get; set; } = new List<BoardColumn>();
}
