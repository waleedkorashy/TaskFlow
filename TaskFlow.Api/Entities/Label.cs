namespace TaskFlow.Api.Entities;

/// <summary>
/// Represents a project-scoped color label that can be applied to many tasks.
/// </summary>
public class Label
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;

    public Project Project { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
