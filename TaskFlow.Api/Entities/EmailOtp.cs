namespace TaskFlow.Api.Entities;

/// <summary>
/// Represents a one-time email verification code issued to a user.
/// </summary>
public class EmailOtp
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Purpose { get; set; } = "EmailVerification";
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
