using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Tasks;

public record UpdateTaskRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    string? Description,
    DateTime? DueDate,
    Guid? AssigneeId);