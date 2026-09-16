using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using TaskFlow.Api.Common.Security;
using TaskFlow.Api.Data;
using TaskFlow.Api.DTOs.Auth;
using TaskFlow.Api.Entities;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly AppDbContext _context;

    public AuthController(
        UserManager<User> userManager,
        ITokenService tokenService,
        IEmailSender emailSender,
        AppDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _context = context;
    }

    private async Task CreateOtpAsync(Guid userId, string purpose, string email)
    {
        var existing = await _context.EmailOtps
            .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed)
            .ToListAsync();

        foreach (var old in existing)
            old.IsUsed = true;

        var code = OtpHelper.GenerateCode();
        var otp = new EmailOtp
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = OtpHelper.HashCode(code),
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.EmailOtps.Add(otp);
        await _context.SaveChangesAsync();
        await _emailSender.SendOtpEmailAsync(email, code);
    }

    private async Task<EmailOtp?> FindValidOtpAsync(Guid userId, string purpose, string code)
    {
        var candidates = await _context.EmailOtps
            .Where(o => o.UserId == userId
                     && o.Purpose == purpose
                     && !o.IsUsed
                     && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return candidates.FirstOrDefault(o => OtpHelper.IsMatch(o.Code, code));
    }

    private static bool IsValid(
        string? email, string? password, string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 100)
            return false;
        if (string.IsNullOrWhiteSpace(password) || password.Length > 128)
            return false;
        return !string.IsNullOrWhiteSpace(email) && email.Length <= 256;
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 100)
            return BadRequest(new { message = "Name must be between 1 and 100 characters." });

        if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
            return BadRequest(new { message = "Please provide a valid email address." });

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return BadRequest(new { message = "Password must be at least 8 characters." });

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return BadRequest(new { message = "Email already registered" });

        var user = new User
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName.Trim()
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = result.Errors.FirstOrDefault()?.Description ?? "Registration failed." });

        await CreateOtpAsync(user.Id, "EmailVerification", user.Email!);

        return Ok(new { message = "Registration successful. Check your email for a verification code." });
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        if (await _userManager.IsLockedOutAsync(user))
            return Unauthorized(new { message = "Too many failed attempts. Please try again in a few minutes." });

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password ?? string.Empty);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);
            return Unauthorized(new { message = "Invalid credentials" });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        if (!user.EmailConfirmed)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Please verify your email address before signing in." });

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Email!, user.FullName, user.Id, user.EmailConfirmed));
    }

    [HttpPost("verify-otp")]
    [EnableRateLimiting("otp")]
    public async Task<ActionResult<AuthResponse>> VerifyOtp(VerifyOtpRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
        if (user == null)
            return BadRequest(new { message = "Invalid or expired code" });

        var otp = await FindValidOtpAsync(user.Id, "EmailVerification", request.Code ?? string.Empty);
        if (otp == null)
            return BadRequest(new { message = "Invalid or expired code" });

        otp.IsUsed = true;
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Email!, user.FullName, user.Id, true));
    }

    [HttpPost("resend-otp")]
    [EnableRateLimiting("otp")]
    public async Task<IActionResult> ResendOtp(ResendOtpRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
        if (user == null || user.EmailConfirmed)
            return BadRequest(new { message = "Invalid request" });

        await CreateOtpAsync(user.Id, "EmailVerification", user.Email!);

        return Ok(new { message = "A new code has been sent." });
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("otp")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);

        if (user != null)
            await CreateOtpAsync(user.Id, "PasswordReset", user.Email!);

        return Ok(new { message = "If that email is registered, a reset code has been sent." });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("otp")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
        if (user == null)
            return BadRequest(new { message = "Invalid or expired code" });

        var otp = await FindValidOtpAsync(user.Id, "PasswordReset", request.Code ?? string.Empty);
        if (otp == null)
            return BadRequest(new { message = "Invalid or expired code" });

        otp.IsUsed = true;

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return BadRequest(new { message = removeResult.Errors.FirstOrDefault()?.Description ?? "Password reset failed." });

        var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword ?? string.Empty);
        if (!addResult.Succeeded)
            return BadRequest(new { message = addResult.Errors.FirstOrDefault()?.Description ?? "Password reset failed." });

        await _context.SaveChangesAsync();

        return Ok(new { message = "Password reset successful. You can now log in with your new password." });
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new System.Net.Mail.MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }
}