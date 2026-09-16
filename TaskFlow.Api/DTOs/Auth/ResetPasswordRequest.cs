using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Auth;

public record ResetPasswordRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(6, MinimumLength = 6)] string Code,
    [Required, StringLength(128, MinimumLength = 8)] string NewPassword);