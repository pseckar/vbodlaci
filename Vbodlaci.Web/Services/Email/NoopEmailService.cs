namespace Vbodlaci.Web.Services.Email;

public sealed class NoopEmailService(ILogger<NoopEmailService> logger) : IEmailService
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Noop email service: message for {Recipient} with subject {Subject} was not sent.",
            message.To,
            message.Subject);
        return Task.CompletedTask;
    }
}
