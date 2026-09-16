using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Comments;

public record CreateCommentRequest([Required, StringLength(5000, MinimumLength = 1)] string Content);