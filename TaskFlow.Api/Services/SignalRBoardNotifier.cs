using Microsoft.AspNetCore.SignalR;
using TaskFlow.Api.Hubs;

namespace TaskFlow.Api.Services;

public class SignalRBoardNotifier : IBoardNotifier
{
    private readonly IHubContext<BoardHub> _hubContext;

    public SignalRBoardNotifier(IHubContext<BoardHub> hubContext)
    {
        _hubContext = hubContext;
    }

    private static string Group(Guid boardId) => BoardHub.GroupName(boardId.ToString());

    public Task NotifyColumnCreatedAsync(Guid boardId, object column) =>
        _hubContext.Clients.Group(Group(boardId)).SendAsync("ColumnCreated", column);

    public Task NotifyColumnRenamedAsync(Guid boardId, object column) =>
        _hubContext.Clients.Group(Group(boardId)).SendAsync("ColumnRenamed", column);

    public Task NotifyColumnDeletedAsync(Guid boardId, Guid columnId) =>
        _hubContext.Clients.Group(Group(boardId)).SendAsync("ColumnDeleted", columnId);

    public Task NotifyTaskCreatedAsync(Guid boardId, object task) =>
        _hubContext.Clients.Group(Group(boardId)).SendAsync("TaskCreated", task);

    public Task NotifyTaskUpdatedAsync(Guid boardId, object task) =>
        _hubContext.Clients.Group(Group(boardId)).SendAsync("TaskUpdated", task);

    public Task NotifyTaskMovedAsync(Guid boardId, object task) =>
        _hubContext.Clients.Group(Group(boardId)).SendAsync("TaskMoved", task);

    public Task NotifyTaskDeletedAsync(Guid boardId, Guid taskId, Guid columnId) =>
        _hubContext.Clients.Group(Group(boardId)).SendAsync("TaskDeleted", new { taskId, columnId });
}