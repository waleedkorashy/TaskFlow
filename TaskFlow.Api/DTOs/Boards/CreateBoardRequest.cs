using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Boards;

public record CreateBoardRequest([Required, StringLength(100, MinimumLength = 1)] string Name);