namespace TaskFlow.Api.DTOs.Tasks;

public record MoveTaskRequest(Guid TargetColumnId, int NewSortOrder);