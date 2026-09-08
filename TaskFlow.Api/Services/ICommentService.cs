using TaskFlow.Api.DTOs.Comments;

namespace TaskFlow.Api.Services;

public interface ICommentService
{
    Task<List<CommentResponse>> GetCommentsAsync(Guid taskId, Guid userId);
    Task<CommentResponse> CreateCommentAsync(Guid taskId, CreateCommentRequest request, Guid userId);
    Task<CommentResponse> UpdateCommentAsync(Guid id, CreateCommentRequest request, Guid userId);
    Task DeleteCommentAsync(Guid id, Guid userId);
}