namespace TaskFlow.Api.Services;

public interface IEmailSender
{
    Task SendOtpEmailAsync(string toEmail, string otpCode);
    Task SendProjectInvitationEmailAsync(string toEmail, string projectName, string inviterName, string acceptUrl);
}