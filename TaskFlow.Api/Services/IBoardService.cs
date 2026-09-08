using TaskFlow.Api.DTOs.Boards;

namespace TaskFlow.Api.Services;

public interface IBoardService
{
    Task<List<BoardResponse>> GetBoardsAsync(Guid projectId, Guid userId);
    Task<BoardResponse> CreateBoardAsync(Guid projectId, CreateBoardRequest request, Guid userId);
    Task<BoardResponse> GetBoardAsync(Guid id, Guid userId);
    Task<BoardResponse> UpdateBoardAsync(Guid id, CreateBoardRequest request, Guid userId);
    Task DeleteBoardAsync(Guid id, Guid userId);
}