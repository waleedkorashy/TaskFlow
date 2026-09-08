using TaskFlow.Api.Common.Exceptions;
using TaskFlow.Api.DTOs.Comments;
using TaskFlow.Api.Entities;
using TaskFlow.Api.Repositories;

namespace TaskFlow.Api.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ITaskRepository _taskRepository;

    public CommentService(ICommentRepository commentRepository, ITaskRepository taskRepository)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
    }

    private static CommentResponse ToResponse(Comment c) => new(
        c.Id, c.TaskItemId, c.UserId, c.User.FullName, c.Content, c.CreatedAt
    );

    private static void EnsureProjectAccess(TaskItem task, Guid userId)
    {
        var project = task.BoardColumn.Board.Project;
        bool hasAccess = project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
        if (!hasAccess) throw new ForbiddenException("You do not have access to this project.");
    }

    public async Task<List<CommentResponse>> GetCommentsAsync(Guid taskId, Guid userId)
    {
        var task = await _taskRepository.GetWithAccessDataAsync(taskId)
            ?? throw new NotFoundException("Task not found.");
        EnsureProjectAccess(task, userId);

        var comments = await _commentRepository.GetByTaskIdAsync(taskId);
        return comments.Select(ToResponse).ToList();
    }

    public async Task<CommentResponse> CreateCommentAsync(Guid taskId, CreateCommentRequest request, Guid userId)
    {
        var task = await _taskRepository.GetWithAccessDataAsync(taskId)
            ?? throw new NotFoundException("Task not found.");
        EnsureProjectAccess(task, userId);

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            TaskItemId = taskId,
            UserId = userId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        var saved = await _commentRepository.GetWithUserAsync(comment.Id);
        return ToResponse(saved!);
    }

    public async Task<CommentResponse> UpdateCommentAsync(Guid id, CreateCommentRequest request, Guid userId)
    {
        var comment = await _commentRepository.GetWithUserAsync(id)
            ?? throw new NotFoundException("Comment not found.");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only edit your own comments.");

        comment.Content = request.Content;
        await _commentRepository.SaveChangesAsync();

        return ToResponse(comment);
    }

    public async Task DeleteCommentAsync(Guid id, Guid userId)
    {
        var comment = await _commentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Comment not found.");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only delete your own comments.");

        _commentRepository.Remove(comment);
        await _commentRepository.SaveChangesAsync();
    }
}