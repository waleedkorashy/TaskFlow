using TaskFlow.Api.Common.Exceptions;
using TaskFlow.Api.DTOs.Boards;
using TaskFlow.Api.Entities;
using TaskFlow.Api.Repositories;

namespace TaskFlow.Api.Services;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;
    private readonly IProjectRepository _projectRepository;

    public BoardService(IBoardRepository boardRepository, IProjectRepository projectRepository)
    {
        _boardRepository = boardRepository;
        _projectRepository = projectRepository;
    }

    private static BoardResponse ToResponse(Board b) => new(
        b.Id, b.ProjectId, b.Name, b.CreatedAt,
        b.BoardColumns.OrderBy(c => c.SortOrder)
            .Select(c => new BoardColumnResponse(c.Id, c.Name, c.SortOrder))
            .ToList()
    );

    private static void EnsureAccess(Project project, Guid userId)
    {
        bool hasAccess = project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
        if (!hasAccess) throw new ForbiddenException("You do not have access to this project.");
    }

    public async Task<List<BoardResponse>> GetBoardsAsync(Guid projectId, Guid userId)
    {
        if (!await _projectRepository.HasAccessAsync(projectId, userId))
            throw new ForbiddenException("You do not have access to this project.");

        var boards = await _boardRepository.GetByProjectIdAsync(projectId);
        return boards.Select(ToResponse).ToList();
    }

    public async Task<BoardResponse> CreateBoardAsync(Guid projectId, CreateBoardRequest request, Guid userId)
    {
        if (!await _projectRepository.HasAccessAsync(projectId, userId))
            throw new ForbiddenException("You do not have access to this project.");

        var board = new Board
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            BoardColumns = new List<BoardColumn>()
        };

        await _boardRepository.AddAsync(board);
        await _boardRepository.SaveChangesAsync();

        return ToResponse(board);
    }

    public async Task<BoardResponse> GetBoardAsync(Guid id, Guid userId)
    {
        var board = await _boardRepository.GetWithDetailsAsync(id)
            ?? throw new NotFoundException("Board not found.");

        EnsureAccess(board.Project, userId);
        return ToResponse(board);
    }

    public async Task<BoardResponse> UpdateBoardAsync(Guid id, CreateBoardRequest request, Guid userId)
    {
        var board = await _boardRepository.GetWithDetailsAsync(id)
            ?? throw new NotFoundException("Board not found.");

        EnsureAccess(board.Project, userId);

        board.Name = request.Name;
        await _boardRepository.SaveChangesAsync();

        return ToResponse(board);
    }

    public async Task DeleteBoardAsync(Guid id, Guid userId)
    {
        var board = await _boardRepository.GetWithDetailsAsync(id)
            ?? throw new NotFoundException("Board not found.");

        EnsureAccess(board.Project, userId);

        _boardRepository.Remove(board);
        await _boardRepository.SaveChangesAsync();
    }
}