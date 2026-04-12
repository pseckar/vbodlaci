namespace Vbodlaci.Web.Services.Email;

public interface IEmailDispatcher
{
    Task<bool> SendAsync(
        string kind,
        string to,
        string subject,
        string textBody,
        string? htmlBody = null,
        CancellationToken cancellationToken = default);
}
