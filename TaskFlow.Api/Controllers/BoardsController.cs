using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs.Boards;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardService;
    public BoardsController(IBoardService boardService) => _boardService = boardService;

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("api/projects/{projectId}/boards")]
    public async Task<ActionResult<IEnumerable<BoardResponse>>> GetBoards(Guid projectId)
    {
        return Ok(await _boardService.GetBoardsAsync(projectId, GetCurrentUserId()));
    }

    [HttpPost("api/projects/{projectId}/boards")]
    public async Task<ActionResult<BoardResponse>> CreateBoard(Guid projectId, CreateBoardRequest request)
    {
        var result = await _boardService.CreateBoardAsync(projectId, request, GetCurrentUserId());
        return CreatedAtAction(nameof(GetBoard), new { id = result.Id }, result);
    }

    [HttpGet("api/boards/{id}")]
    public async Task<ActionResult<BoardResponse>> GetBoard(Guid id)
    {
        return Ok(await _boardService.GetBoardAsync(id, GetCurrentUserId()));
    }

    [HttpPut("api/boards/{id}")]
    public async Task<ActionResult<BoardResponse>> UpdateBoard(Guid id, CreateBoardRequest request)
    {
        return Ok(await _boardService.UpdateBoardAsync(id, request, GetCurrentUserId()));
    }

    [HttpDelete("api/boards/{id}")]
    public async Task<IActionResult> DeleteBoard(Guid id)
    {
        await _boardService.DeleteBoardAsync(id, GetCurrentUserId());
        return NoContent();
    }
}