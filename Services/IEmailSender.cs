namespace FYP_25_S3_15P.Services;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, string? plainTextBody = null);
}