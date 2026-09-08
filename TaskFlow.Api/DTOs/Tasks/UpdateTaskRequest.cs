namespace TaskFlow.Api.DTOs.Tasks;

public record UpdateTaskRequest(string Title, string? Description, DateTime? DueDate, Guid? AssigneeId);