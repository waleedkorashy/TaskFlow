using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TaskFlow.Api.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_configuration["Email:From"]!)); message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Verify your TaskFlow account";
        message.Body = new TextPart("plain")
        {
            Text = $"Your verification code is: {otpCode}\n\nThis code expires in 10 minutes."
        };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["Email:Host"]!,
                int.Parse(_configuration["Email:Port"]!),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_configuration["Email:Username"]!, _configuration["Email:Password"]!); await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deliver OTP email to {ToEmail} via {SmtpHost}. The registration still succeeded; the user can request a resend.",
                toEmail, _configuration["Email:Host"]);
        }
    }
    public async Task SendProjectInvitationEmailAsync(string toEmail, string projectName, string inviterName, string acceptUrl)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_configuration["Email:From"]!));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = $"{inviterName} invited you to \"{projectName}\" on TaskFlow";
        message.Body = new TextPart("plain")
        {
            Text = $"{inviterName} has invited you to collaborate on the project \"{projectName}\" on TaskFlow.\n\n" +
                   $"Accept the invitation here: {acceptUrl}\n\n" +
                   $"This invitation expires in 7 days. If you don't have a TaskFlow account yet, you'll be asked to create one first."
        };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_configuration["Email:Host"]!, int.Parse(_configuration["Email:Port"]!), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_configuration["Email:Username"]!, _configuration["Email:Password"]!);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deliver invitation email to {ToEmail} via {SmtpHost}.", toEmail, _configuration["Email:Host"]);
        }
    }
}