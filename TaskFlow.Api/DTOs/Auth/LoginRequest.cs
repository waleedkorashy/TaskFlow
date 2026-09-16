using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Auth;

public record LoginRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128, MinimumLength = 8)] string Password);