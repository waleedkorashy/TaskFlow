namespace TaskFlow.Api.DTOs.Tasks;

public record TaskResponse(
    Guid Id,
    Guid BoardColumnId,
    string Title,
    string? Description,
    int SortOrder,
    DateTime? DueDate,
    Guid? AssigneeId,
    string? AssigneeName,
    DateTime CreatedAt
);