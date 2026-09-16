using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Projects;

public record CreateProjectRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    string? Description);