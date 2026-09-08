using TaskFlow.Api.Common.Exceptions;
using TaskFlow.Api.DTOs.Tasks;
using TaskFlow.Api.Entities;
using TaskFlow.Api.Repositories;

namespace TaskFlow.Api.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IColumnRepository _columnRepository;
    private readonly IBoardNotifier _notifier;

    public TaskService(ITaskRepository taskRepository, IColumnRepository columnRepository, IBoardNotifier notifier)
    {
        _taskRepository = taskRepository;
        _columnRepository = columnRepository;
        _notifier = notifier;
    }

    private static TaskResponse ToResponse(TaskItem t) => new(
        t.Id, t.BoardColumnId, t.Title, t.Description, t.SortOrder,
        t.DueDate, t.AssigneeId, t.Assignee?.FullName, t.CreatedAt
    );

    private static void EnsureAccess(Project project, Guid userId)
    {
        bool hasAccess = project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
        if (!hasAccess) throw new ForbiddenException("You do not have access to this project.");
    }

    public async Task<List<TaskResponse>> GetTasksAsync(Guid columnId, Guid userId)
    {
        var column = await _columnRepository.GetWithAccessDataAsync(columnId)
            ?? throw new NotFoundException("Column not found.");
        EnsureAccess(column.Board.Project, userId);

        var tasks = await _taskRepository.GetByColumnIdOrderedAsync(columnId);
        return tasks.Select(ToResponse).ToList();
    }

    public async Task<TaskResponse> GetTaskAsync(Guid id, Guid userId)
    {
        var task = await _taskRepository.GetWithAccessDataAsync(id)
            ?? throw new NotFoundException("Task not found.");
        EnsureAccess(task.BoardColumn.Board.Project, userId);
        return ToResponse(task);
    }

    public async Task<TaskResponse> CreateTaskAsync(Guid columnId, CreateTaskRequest request, Guid userId)
    {
        var column = await _columnRepository.GetWithTasksAndAccessDataAsync(columnId)
            ?? throw new NotFoundException("Column not found.");
        EnsureAccess(column.Board.Project, userId);

        int nextOrder = column.Tasks.Count == 0 ? 0 : column.Tasks.Max(t => t.SortOrder) + 1;

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            BoardColumnId = columnId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            AssigneeId = request.AssigneeId,
            SortOrder = nextOrder,
            CreatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        var saved = await _taskRepository.GetWithAccessDataAsync(task.Id);
        await _notifier.NotifyTaskCreatedAsync(column.Board.Id, ToResponse(saved!));
        return ToResponse(saved!);
    }

    public async Task<TaskResponse> UpdateTaskAsync(Guid id, UpdateTaskRequest request, Guid userId)
    {
        var task = await _taskRepository.GetWithAccessDataAsync(id)
            ?? throw new NotFoundException("Task not found.");
        EnsureAccess(task.BoardColumn.Board.Project, userId);

        task.Title = request.Title;
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.AssigneeId = request.AssigneeId;

        await _taskRepository.SaveChangesAsync();

        var updated = await _taskRepository.GetWithAccessDataAsync(id);
        await _notifier.NotifyTaskUpdatedAsync(updated!.BoardColumn.Board.Id, ToResponse(updated!));
        return ToResponse(updated!);
    }

    public async Task<TaskResponse> MoveTaskAsync(Guid id, MoveTaskRequest request, Guid userId)
    {
        var task = await _taskRepository.GetWithAccessDataAsync(id)
            ?? throw new NotFoundException("Task not found.");
        EnsureAccess(task.BoardColumn.Board.Project, userId);

        var targetColumn = await _columnRepository.GetWithAccessDataAsync(request.TargetColumnId)
            ?? throw new NotFoundException("Target column not found.");
        EnsureAccess(targetColumn.Board.Project, userId);

        var sourceColumnId = task.BoardColumnId;
        bool sameColumn = sourceColumnId == request.TargetColumnId;

        var sourceTasks = await _taskRepository.GetByColumnIdOrderedAsync(sourceColumnId);
        sourceTasks = sourceTasks.Where(t => t.Id != id).ToList();

        var targetTasks = sameColumn
            ? sourceTasks
            : await _taskRepository.GetByColumnIdOrderedAsync(request.TargetColumnId);

        task.BoardColumnId = request.TargetColumnId;
        int insertAt = Math.Clamp(request.NewSortOrder, 0, targetTasks.Count);
        targetTasks.Insert(insertAt, task);

        for (int i = 0; i < targetTasks.Count; i++)
            targetTasks[i].SortOrder = i;

        if (!sameColumn)
        {
            for (int i = 0; i < sourceTasks.Count; i++)
                sourceTasks[i].SortOrder = i;
        }

        await _taskRepository.SaveChangesAsync();

        var moved = await _taskRepository.GetWithAccessDataAsync(id);
        await _notifier.NotifyTaskMovedAsync(moved!.BoardColumn.Board.Id, ToResponse(moved!));
        return ToResponse(moved!);
    }

    public async Task DeleteTaskAsync(Guid id, Guid userId)
    {
        var task = await _taskRepository.GetWithAccessDataAsync(id)
            ?? throw new NotFoundException("Task not found.");
        EnsureAccess(task.BoardColumn.Board.Project, userId);

        var boardId = task.BoardColumn.Board.Id;
        var columnId = task.BoardColumnId;

        _taskRepository.Remove(task);
        await _taskRepository.SaveChangesAsync();

        var remainingTasks = await _taskRepository.GetByColumnIdOrderedAsync(columnId);
        for (int i = 0; i < remainingTasks.Count; i++)
            remainingTasks[i].SortOrder = i;

        await _taskRepository.SaveChangesAsync();

        await _notifier.NotifyTaskDeletedAsync(boardId, id, columnId);
    }
}