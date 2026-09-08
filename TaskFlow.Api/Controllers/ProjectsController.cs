using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.DTOs.Projects;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    public ProjectsController(IProjectService projectService) => _projectService = projectService;

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetProjects()
    {
        return Ok(await _projectService.GetUserProjectsAsync(GetCurrentUserId()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectResponse>> GetProject(Guid id)
    {
        return Ok(await _projectService.GetProjectAsync(id, GetCurrentUserId()));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> CreateProject(CreateProjectRequest request)
    {
        var result = await _projectService.CreateProjectAsync(request, GetCurrentUserId());
        return CreatedAtAction(nameof(GetProject), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectResponse>> UpdateProject(Guid id, CreateProjectRequest request)
    {
        return Ok(await _projectService.UpdateProjectAsync(id, request, GetCurrentUserId()));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        await _projectService.DeleteProjectAsync(id, GetCurrentUserId());
        return NoContent();
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<List<ProjectMemberResponse>>> GetMembers(Guid id)
    {
        return Ok(await _projectService.GetMembersAsync(id, GetCurrentUserId()));
    }

    [HttpDelete("{projectId}/members/{memberUserId}")]
    public async Task<IActionResult> RemoveMember(Guid projectId, Guid memberUserId)
    {
        await _projectService.RemoveMemberAsync(projectId, memberUserId, GetCurrentUserId());
        return NoContent();
    }

    [HttpPost("{projectId}/leave")]
    public async Task<IActionResult> LeaveProject(Guid projectId)
    {
        await _projectService.LeaveProjectAsync(projectId, GetCurrentUserId());
        return NoContent();
    }
}