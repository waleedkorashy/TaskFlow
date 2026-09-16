using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs.Dashboard;

namespace TaskFlow.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardStatsResponse>> GetStats()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var projects = await _db.Projects
            .Include(p => p.Boards).ThenInclude(b => b.BoardColumns).ThenInclude(c => c.Tasks)
            .Include(p => p.Members)
            .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var summaries = projects
            .Select(p => new ProjectSummary(
                p.Id,
                p.Name,
                p.CreatedAt,
                p.Boards.Count,
                p.Boards.SelectMany(b => b.BoardColumns).SelectMany(c => c.Tasks).Distinct().Count(),
                p.Members.Count + 1
            ))
            .ToList();

        var response = new DashboardStatsResponse(
            summaries.Count,
            summaries.Sum(p => p.BoardCount),
            summaries.Sum(p => p.TaskCount),
            summaries.Take(5).ToList(),
            summaries
        );

        return Ok(response);
    }
}