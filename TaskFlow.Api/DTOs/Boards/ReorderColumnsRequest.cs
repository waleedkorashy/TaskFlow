namespace TaskFlow.Api.DTOs.Boards;

public record ReorderColumnsRequest(List<Guid> OrderedColumnIds);