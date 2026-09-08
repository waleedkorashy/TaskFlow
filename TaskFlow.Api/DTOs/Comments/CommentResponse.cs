namespace TaskFlow.Api.DTOs.Comments;

public record CommentResponse(
    Guid Id,
    Guid TaskItemId,
    Guid UserId,
    string UserName,
    string Content,
    DateTime CreatedAt
);