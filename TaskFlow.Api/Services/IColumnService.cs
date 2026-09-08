using TaskFlow.Api.DTOs.Boards;

namespace TaskFlow.Api.Services;

public interface IColumnService
{
    Task<BoardColumnResponse> CreateColumnAsync(Guid boardId, CreateColumnRequest request, Guid userId);
    Task<BoardColumnResponse> RenameColumnAsync(Guid id, CreateColumnRequest request, Guid userId);
    Task DeleteColumnAsync(Guid id, Guid userId);
    Task ReorderColumnsAsync(Guid boardId, ReorderColumnsRequest request, Guid userId);
}