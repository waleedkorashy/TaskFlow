using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Labels;

public record CreateLabelRequest(
    [Required, StringLength(50, MinimumLength = 1)] string Name,
    [Required, RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "ColorHex must be a valid hex color like #FF5733.")] string ColorHex);