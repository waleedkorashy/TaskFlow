namespace TaskFlow.Api.DTOs.Boards;

public record BoardResponse(
    Guid Id,
    Guid ProjectId,
    string Name,
    DateTime CreatedAt,
    List<BoardColumnResponse> Columns
);