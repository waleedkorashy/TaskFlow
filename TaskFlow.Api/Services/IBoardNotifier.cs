namespace TaskFlow.Api.Services;

public interface IBoardNotifier
{
    Task NotifyColumnCreatedAsync(Guid boardId, object column);
    Task NotifyColumnRenamedAsync(Guid boardId, object column);
    Task NotifyColumnDeletedAsync(Guid boardId, Guid columnId);
    Task NotifyTaskCreatedAsync(Guid boardId, object task);
    Task NotifyTaskUpdatedAsync(Guid boardId, object task);
    Task NotifyTaskMovedAsync(Guid boardId, object task);
    Task NotifyTaskDeletedAsync(Guid boardId, Guid taskId, Guid columnId);
}