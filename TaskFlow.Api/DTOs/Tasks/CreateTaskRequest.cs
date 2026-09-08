namespace TaskFlow.Api.DTOs.Tasks;

public record CreateTaskRequest(string Title, string? Description, DateTime? DueDate, Guid? AssigneeId);