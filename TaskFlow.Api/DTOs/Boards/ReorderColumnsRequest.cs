using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Boards;

public record ReorderColumnsRequest([Required, MinLength(1)] List<Guid> OrderedColumnIds);