using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.DTOs.Auth;

public record ResendOtpRequest([Required, EmailAddress, StringLength(256)] string Email);