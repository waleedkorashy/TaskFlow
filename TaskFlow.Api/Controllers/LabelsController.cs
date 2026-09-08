using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs.Labels;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
public class LabelsController : ControllerBase
{
    private readonly ILabelService _labelService;
    public LabelsController(ILabelService labelService) => _labelService = labelService;

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("api/projects/{projectId}/labels")]
    public async Task<ActionResult<IEnumerable<LabelResponse>>> GetLabels(Guid projectId)
    {
        return Ok(await _labelService.GetLabelsAsync(projectId, GetCurrentUserId()));
    }

    [HttpPost("api/projects/{projectId}/labels")]
    public async Task<ActionResult<LabelResponse>> CreateLabel(Guid projectId, CreateLabelRequest request)
    {
        return Ok(await _labelService.CreateLabelAsync(projectId, request, GetCurrentUserId()));
    }

    [HttpPut("api/labels/{id}")]
    public async Task<ActionResult<LabelResponse>> UpdateLabel(Guid id, CreateLabelRequest request)
    {
        return Ok(await _labelService.UpdateLabelAsync(id, request, GetCurrentUserId()));
    }

    [HttpDelete("api/labels/{id}")]
    public async Task<IActionResult> DeleteLabel(Guid id)
    {
        await _labelService.DeleteLabelAsync(id, GetCurrentUserId());
        return NoContent();
    }

    [HttpPost("api/tasks/{taskId}/labels/{labelId}")]
    public async Task<IActionResult> AttachLabel(Guid taskId, Guid labelId)
    {
        await _labelService.AttachLabelAsync(taskId, labelId, GetCurrentUserId());
        return NoContent();
    }

    [HttpDelete("api/tasks/{taskId}/labels/{labelId}")]
    public async Task<IActionResult> DetachLabel(Guid taskId, Guid labelId)
    {
        await _labelService.DetachLabelAsync(taskId, labelId, GetCurrentUserId());
        return NoContent();
    }
}