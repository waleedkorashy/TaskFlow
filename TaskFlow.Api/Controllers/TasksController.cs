using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs.Tasks;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    public TasksController(ITaskService taskService) => _taskService = taskService;

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("api/columns/{columnId}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetTasks(Guid columnId)
    {
        return Ok(await _taskService.GetTasksAsync(columnId, GetCurrentUserId()));
    }

    [HttpGet("api/tasks/{id}")]
    public async Task<ActionResult<TaskResponse>> GetTask(Guid id)
    {
        return Ok(await _taskService.GetTaskAsync(id, GetCurrentUserId()));
    }

    [HttpPost("api/columns/{columnId}/tasks")]
    public async Task<ActionResult<TaskResponse>> CreateTask(Guid columnId, CreateTaskRequest request)
    {
        var result = await _taskService.CreateTaskAsync(columnId, request, GetCurrentUserId());
        return CreatedAtAction(nameof(GetTask), new { id = result.Id }, result);
    }

    [HttpPut("api/tasks/{id}")]
    public async Task<ActionResult<TaskResponse>> UpdateTask(Guid id, UpdateTaskRequest request)
    {
        return Ok(await _taskService.UpdateTaskAsync(id, request, GetCurrentUserId()));
    }

    [HttpPut("api/tasks/{id}/move")]
    public async Task<ActionResult<TaskResponse>> MoveTask(Guid id, MoveTaskRequest request)
    {
        return Ok(await _taskService.MoveTaskAsync(id, request, GetCurrentUserId()));
    }

    [HttpDelete("api/tasks/{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        await _taskService.DeleteTaskAsync(id, GetCurrentUserId());
        return NoContent();
    }
}