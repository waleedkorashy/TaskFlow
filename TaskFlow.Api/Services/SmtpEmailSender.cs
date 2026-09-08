using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TaskFlow.Api.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
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

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _configuration["Email:Host"]!,
            int.Parse(_configuration["Email:Port"]!),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(_configuration["Email:Username"]!, _configuration["Email:Password"]!); await client.SendAsync(message);
        await client.DisconnectAsync(true);
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

        using var client = new SmtpClient();
        await client.ConnectAsync(_configuration["Email:Host"]!, int.Parse(_configuration["Email:Port"]!), SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_configuration["Email:Username"]!, _configuration["Email:Password"]!);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}