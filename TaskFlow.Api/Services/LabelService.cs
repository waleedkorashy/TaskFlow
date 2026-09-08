using TaskFlow.Api.Common.Exceptions;
using TaskFlow.Api.DTOs.Labels;
using TaskFlow.Api.Entities;
using TaskFlow.Api.Repositories;

namespace TaskFlow.Api.Services;

public class LabelService : ILabelService
{
    private readonly ILabelRepository _labelRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public LabelService(ILabelRepository labelRepository, IProjectRepository projectRepository, ITaskRepository taskRepository)
    {
        _labelRepository = labelRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
    }

    private static LabelResponse ToResponse(Label l) => new(l.Id, l.ProjectId, l.Name, l.ColorHex);

    private async Task EnsureProjectAccess(Guid projectId, Guid userId)
    {
        if (!await _projectRepository.HasAccessAsync(projectId, userId))
            throw new ForbiddenException("You do not have access to this project.");
    }

    public async Task<List<LabelResponse>> GetLabelsAsync(Guid projectId, Guid userId)
    {
        await EnsureProjectAccess(projectId, userId);
        var labels = await _labelRepository.GetByProjectIdAsync(projectId);
        return labels.Select(ToResponse).ToList();
    }

    public async Task<LabelResponse> CreateLabelAsync(Guid projectId, CreateLabelRequest request, Guid userId)
    {
        await EnsureProjectAccess(projectId, userId);

        var label = new Label
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = request.Name,
            ColorHex = request.ColorHex
        };

        await _labelRepository.AddAsync(label);
        await _labelRepository.SaveChangesAsync();

        return ToResponse(label);
    }

    public async Task<LabelResponse> UpdateLabelAsync(Guid id, CreateLabelRequest request, Guid userId)
    {
        var label = await _labelRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Label not found.");
        await EnsureProjectAccess(label.ProjectId, userId);

        label.Name = request.Name;
        label.ColorHex = request.ColorHex;
        await _labelRepository.SaveChangesAsync();

        return ToResponse(label);
    }

    public async Task DeleteLabelAsync(Guid id, Guid userId)
    {
        var label = await _labelRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Label not found.");
        await EnsureProjectAccess(label.ProjectId, userId);

        _labelRepository.Remove(label);
        await _labelRepository.SaveChangesAsync();
    }

    public async Task AttachLabelAsync(Guid taskId, Guid labelId, Guid userId)
    {
        var task = await _taskRepository.GetWithAccessDataAsync(taskId)
            ?? throw new NotFoundException("Task not found.");

        var project = task.BoardColumn.Board.Project;
        bool hasAccess = project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
        if (!hasAccess) throw new ForbiddenException("You do not have access to this project.");

        var label = await _labelRepository.GetByIdAndProjectAsync(labelId, project.Id)
            ?? throw new NotFoundException("Label not found in this project.");

        if (!task.Labels.Any(l => l.Id == labelId))
        {
            task.Labels.Add(label);
            await _taskRepository.SaveChangesAsync();
        }
    }

    public async Task DetachLabelAsync(Guid taskId, Guid labelId, Guid userId)
    {
        var task = await _taskRepository.GetWithAccessDataAsync(taskId)
            ?? throw new NotFoundException("Task not found.");

        var project = task.BoardColumn.Board.Project;
        bool hasAccess = project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
        if (!hasAccess) throw new ForbiddenException("You do not have access to this project.");

        var label = task.Labels.FirstOrDefault(l => l.Id == labelId);
        if (label != null)
        {
            task.Labels.Remove(label);
            await _taskRepository.SaveChangesAsync();
        }
    }
}