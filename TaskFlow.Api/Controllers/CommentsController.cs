using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs.Comments;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    public CommentsController(ICommentService commentService) => _commentService = commentService;

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("api/tasks/{taskId}/comments")]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetComments(Guid taskId)
    {
        return Ok(await _commentService.GetCommentsAsync(taskId, GetCurrentUserId()));
    }

    [HttpPost("api/tasks/{taskId}/comments")]
    public async Task<ActionResult<CommentResponse>> CreateComment(Guid taskId, CreateCommentRequest request)
    {
        var result = await _commentService.CreateCommentAsync(taskId, request, GetCurrentUserId());
        return CreatedAtAction(nameof(GetComments), new { taskId }, result);
    }

    [HttpPut("api/comments/{id}")]
    public async Task<ActionResult<CommentResponse>> UpdateComment(Guid id, CreateCommentRequest request)
    {
        return Ok(await _commentService.UpdateCommentAsync(id, request, GetCurrentUserId()));
    }

    [HttpDelete("api/comments/{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        await _commentService.DeleteCommentAsync(id, GetCurrentUserId());
        return NoContent();
    }
}