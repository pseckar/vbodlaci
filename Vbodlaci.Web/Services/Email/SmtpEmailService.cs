using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Vbodlaci.Web.Services.Email;

public sealed class SmtpEmailService(IOptions<SmtpOptions> options, ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpOptions optionsValue = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(optionsValue.Host))
        {
            throw new InvalidOperationException("SMTP host is not configured.");
        }

        using var smtpClient = new SmtpClient(optionsValue.Host, optionsValue.Port)
        {
            EnableSsl = optionsValue.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(optionsValue.UserName))
        {
            smtpClient.Credentials = new NetworkCredential(optionsValue.UserName, optionsValue.Password);
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(optionsValue.From),
            Subject = message.Subject,
            Body = string.IsNullOrWhiteSpace(message.HtmlBody) ? message.TextBody ?? string.Empty : message.HtmlBody,
            IsBodyHtml = !string.IsNullOrWhiteSpace(message.HtmlBody)
        };
        mailMessage.To.Add(message.To);

        logger.LogInformation("Sending email to {Recipient} via SMTP host {Host}.", message.To, optionsValue.Host);
        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }
}
