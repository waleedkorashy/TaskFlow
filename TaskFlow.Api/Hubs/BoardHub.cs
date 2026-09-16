using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Api.Repositories;

namespace TaskFlow.Api.Hubs;

[Authorize]
public class BoardHub : Hub
{
    private readonly IBoardRepository _boardRepository;

    public BoardHub(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task JoinBoard(string boardId)
    {
        if (Guid.TryParse(boardId, out var parsedId) &&
            GetUserId() is Guid userId &&
            await _boardRepository.HasAccessAsync(parsedId, userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(boardId));
            return;
        }

        throw new HubException("You do not have access to this board.");
    }

    public async Task LeaveBoard(string boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(boardId));
    }

    public static string GroupName(string boardId) => $"board-{boardId}";

    private Guid? GetUserId()
    {
        var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}