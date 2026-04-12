using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Emails;

namespace Vbodlaci.Web.Services.Email;

public sealed class EmailDispatcher(
    IEmailService emailService,
    ApplicationDbContext dbContext,
    ILogger<EmailDispatcher> logger) : IEmailDispatcher
{
    public async Task<bool> SendAsync(
        string kind,
        string to,
        string subject,
        string textBody,
        string? htmlBody = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await emailService.SendAsync(new EmailMessage
            {
                To = to,
                Subject = subject,
                TextBody = textBody,
                HtmlBody = htmlBody
            }, cancellationToken);

            dbContext.EmailLogs.Add(new EmailLog
            {
                Kind = kind,
                Recipient = to,
                Subject = subject,
                Status = "Sent",
                ErrorMessage = string.Empty,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Email send failed for {Recipient} ({Kind}).", to, kind);
            dbContext.EmailLogs.Add(new EmailLog
            {
                Kind = kind,
                Recipient = to,
                Subject = subject,
                Status = "Failed",
                ErrorMessage = ex.Message,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }
    }
}
