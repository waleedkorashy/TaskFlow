using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Boards;

public record CreateColumnRequest([Required, StringLength(50, MinimumLength = 1)] string Name);