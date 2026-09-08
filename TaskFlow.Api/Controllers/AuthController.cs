using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
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

    private static string GenerateOtpCode()
    {
        return Random.Shared.Next(0, 1000000).ToString("D6");
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return BadRequest(new { message = "Email already registered" });

        var user = new User
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var code = GenerateOtpCode();
        var otp = new EmailOtp
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Code = code,
            Purpose = "EmailVerification",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.EmailOtps.Add(otp);
        await _context.SaveChangesAsync();

        await _emailSender.SendOtpEmailAsync(user.Email!, code);

        return Ok(new { message = "Registration successful. Check your email for a verification code." });
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Unauthorized(new { message = "Invalid credentials" });

        if (!user.EmailConfirmed)
            return Forbid();

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Email!, user.FullName, user.Id, user.EmailConfirmed));
    }

    [HttpPost("verify-otp")]
    [EnableRateLimiting("otp")]
    public async Task<ActionResult<AuthResponse>> VerifyOtp(VerifyOtpRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { message = "Invalid request" });

        var otp = await _context.EmailOtps
            .Where(o => o.UserId == user.Id
                     && o.Code == request.Code
                     && o.Purpose == "EmailVerification"
                     && !o.IsUsed
                     && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

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
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.EmailConfirmed)
            return BadRequest(new { message = "Invalid request" });

        var code = GenerateOtpCode();
        var otp = new EmailOtp
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Code = code,
            Purpose = "EmailVerification",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.EmailOtps.Add(otp);
        await _context.SaveChangesAsync();

        await _emailSender.SendOtpEmailAsync(user.Email!, code);

        return Ok(new { message = "A new code has been sent." });
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("otp")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user != null)
        {
            var code = GenerateOtpCode();
            var otp = new EmailOtp
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Code = code,
                Purpose = "PasswordReset",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmailOtps.Add(otp);
            await _context.SaveChangesAsync();

            await _emailSender.SendOtpEmailAsync(user.Email!, code);
        }

  
        return Ok(new { message = "If that email is registered, a reset code has been sent." });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("otp")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { message = "Invalid or expired code" });

        var otp = await _context.EmailOtps
            .Where(o => o.UserId == user.Id
                     && o.Code == request.Code
                     && o.Purpose == "PasswordReset"
                     && !o.IsUsed
                     && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null)
            return BadRequest(new { message = "Invalid or expired code" });

        otp.IsUsed = true;

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return BadRequest(removeResult.Errors);

        var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addResult.Succeeded)
            return BadRequest(addResult.Errors);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Password reset successful. You can now log in with your new password." });
    }
}