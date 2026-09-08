namespace TaskFlow.Api.DTOs.Auth;

public record VerifyOtpRequest(string Email, string Code);