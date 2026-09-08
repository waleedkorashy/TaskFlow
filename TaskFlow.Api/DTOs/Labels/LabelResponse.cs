namespace TaskFlow.Api.DTOs.Labels;

public record LabelResponse(Guid Id, Guid ProjectId, string Name, string ColorHex);