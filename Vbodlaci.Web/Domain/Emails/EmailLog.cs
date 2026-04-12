namespace Vbodlaci.Web.Domain.Emails;

public sealed class EmailLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Kind { get; set; } = string.Empty;

    public string Recipient { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
