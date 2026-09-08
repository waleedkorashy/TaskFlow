using TaskFlow.Api.DTOs.Labels;

namespace TaskFlow.Api.Services;

public interface ILabelService
{
    Task<List<LabelResponse>> GetLabelsAsync(Guid projectId, Guid userId);
    Task<LabelResponse> CreateLabelAsync(Guid projectId, CreateLabelRequest request, Guid userId);
    Task<LabelResponse> UpdateLabelAsync(Guid id, CreateLabelRequest request, Guid userId);
    Task DeleteLabelAsync(Guid id, Guid userId);
    Task AttachLabelAsync(Guid taskId, Guid labelId, Guid userId);
    Task DetachLabelAsync(Guid taskId, Guid labelId, Guid userId);
}