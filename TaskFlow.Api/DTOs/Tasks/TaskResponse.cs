using TaskFlow.Api.DTOs.Labels;

namespace TaskFlow.Api.DTOs.Tasks;

public record TaskResponse(
    Guid Id,
    Guid BoardColumnId,
    Guid BoardId,
    Guid ProjectId,
    string Title,
    string? Description,
    int SortOrder,
    DateTime? DueDate,
    Guid? AssigneeId,
    string? AssigneeName,
    DateTime CreatedAt,
    List<LabelResponse> Labels
);