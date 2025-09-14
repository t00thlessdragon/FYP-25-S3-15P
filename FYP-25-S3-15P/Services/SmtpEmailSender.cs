using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace FYP_25_S3_15P.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _opt;
    public SmtpEmailSender(IOptions<SmtpOptions> options) => _opt = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, string? plainTextBody = null)
    {
        using var msg = new MailMessage
        {
            From = new MailAddress(_opt.FromEmail, _opt.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        msg.To.Add(to);

        if (!string.IsNullOrWhiteSpace(plainTextBody))
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain"));

        using var client = new SmtpClient(_opt.Host, _opt.Port)
        {
            EnableSsl = _opt.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_opt.User, _opt.Password),
            Timeout = 10000
        };

        await client.SendMailAsync(msg);
    }
}