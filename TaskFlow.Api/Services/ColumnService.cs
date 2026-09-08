using TaskFlow.Api.Common.Exceptions;
using TaskFlow.Api.DTOs.Boards;
using TaskFlow.Api.Entities;
using TaskFlow.Api.Repositories;

namespace TaskFlow.Api.Services;

public class ColumnService : IColumnService
{
    private readonly IColumnRepository _columnRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardNotifier _notifier;

    public ColumnService(IColumnRepository columnRepository, IBoardRepository boardRepository, IBoardNotifier notifier)
    {
        _columnRepository = columnRepository;
        _boardRepository = boardRepository;
        _notifier = notifier;
    }

    private static void EnsureAccess(Project project, Guid userId)
    {
        bool hasAccess = project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
        if (!hasAccess) throw new ForbiddenException("You do not have access to this project.");
    }

    public async Task<BoardColumnResponse> CreateColumnAsync(Guid boardId, CreateColumnRequest request, Guid userId)
    {
        var board = await _boardRepository.GetWithDetailsAsync(boardId)
            ?? throw new NotFoundException("Board not found.");

        EnsureAccess(board.Project, userId);

        int nextOrder = board.BoardColumns.Count == 0 ? 0 : board.BoardColumns.Max(c => c.SortOrder) + 1;

        var column = new BoardColumn
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            Name = request.Name,
            SortOrder = nextOrder
        };

        await _columnRepository.AddAsync(column);
        await _columnRepository.SaveChangesAsync();
        await _notifier.NotifyColumnCreatedAsync(column.BoardId, new BoardColumnResponse(column.Id, column.Name, column.SortOrder));
        return new BoardColumnResponse(column.Id, column.Name, column.SortOrder);
    }

    public async Task<BoardColumnResponse> RenameColumnAsync(Guid id, CreateColumnRequest request, Guid userId)
    {
        var column = await _columnRepository.GetWithAccessDataAsync(id)
            ?? throw new NotFoundException("Column not found.");
        EnsureAccess(column.Board.Project, userId);
        column.Name = request.Name;
        await _columnRepository.SaveChangesAsync();

        await _notifier.NotifyColumnRenamedAsync(column.BoardId, new BoardColumnResponse(column.Id, column.Name, column.SortOrder));

        return new BoardColumnResponse(column.Id, column.Name, column.SortOrder);
    }

    public async Task DeleteColumnAsync(Guid id, Guid userId)
    {
        var column = await _columnRepository.GetWithAccessDataAsync(id)
            ?? throw new NotFoundException("Column not found.");

        EnsureAccess(column.Board.Project, userId);
        var boardId = column.BoardId;
        _columnRepository.Remove(column);
        await _columnRepository.SaveChangesAsync();
        await _notifier.NotifyColumnDeletedAsync(boardId, id);
    }

    public async Task ReorderColumnsAsync(Guid boardId, ReorderColumnsRequest request, Guid userId)
    {
        var board = await _boardRepository.GetWithDetailsAsync(boardId)
            ?? throw new NotFoundException("Board not found.");

        EnsureAccess(board.Project, userId);

        for (int i = 0; i < request.OrderedColumnIds.Count; i++)
        {
            var column = board.BoardColumns.FirstOrDefault(c => c.Id == request.OrderedColumnIds[i]);
            if (column != null) column.SortOrder = i;
        }

        await _boardRepository.SaveChangesAsync();
    }
}