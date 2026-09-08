using TaskFlow.Api.DTOs.Tasks;

namespace TaskFlow.Api.Services;

public interface ITaskService
{
    Task<List<TaskResponse>> GetTasksAsync(Guid columnId, Guid userId);
    Task<TaskResponse> GetTaskAsync(Guid id, Guid userId);
    Task<TaskResponse> CreateTaskAsync(Guid columnId, CreateTaskRequest request, Guid userId);
    Task<TaskResponse> UpdateTaskAsync(Guid id, UpdateTaskRequest request, Guid userId);
    Task<TaskResponse> MoveTaskAsync(Guid id, MoveTaskRequest request, Guid userId);
    Task DeleteTaskAsync(Guid id, Guid userId);
}