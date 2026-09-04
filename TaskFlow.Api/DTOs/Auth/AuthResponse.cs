namespace TaskFlow.Api.DTOs.Auth;

public record AuthResponse(string Token, string Email, string FullName, Guid UserId);
