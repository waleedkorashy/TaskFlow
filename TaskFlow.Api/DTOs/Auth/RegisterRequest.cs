using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Auth;

public record RegisterRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128, MinimumLength = 8)] string Password,
    [Required, StringLength(100, MinimumLength = 1)] string FullName);