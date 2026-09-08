using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs.Boards;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
public class ColumnsController : ControllerBase
{
    private readonly IColumnService _columnService;
    public ColumnsController(IColumnService columnService) => _columnService = columnService;

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("api/boards/{boardId}/columns")]
    public async Task<ActionResult<BoardColumnResponse>> CreateColumn(Guid boardId, CreateColumnRequest request)
    {
        return Ok(await _columnService.CreateColumnAsync(boardId, request, GetCurrentUserId()));
    }

    [HttpPut("api/columns/{id}")]
    public async Task<ActionResult<BoardColumnResponse>> RenameColumn(Guid id, CreateColumnRequest request)
    {
        return Ok(await _columnService.RenameColumnAsync(id, request, GetCurrentUserId()));
    }

    [HttpDelete("api/columns/{id}")]
    public async Task<IActionResult> DeleteColumn(Guid id)
    {
        await _columnService.DeleteColumnAsync(id, GetCurrentUserId());
        return NoContent();
    }

    [HttpPut("api/boards/{boardId}/columns/reorder")]
    public async Task<IActionResult> ReorderColumns(Guid boardId, ReorderColumnsRequest request)
    {
        await _columnService.ReorderColumnsAsync(boardId, request, GetCurrentUserId());
        return NoContent();
    }
}