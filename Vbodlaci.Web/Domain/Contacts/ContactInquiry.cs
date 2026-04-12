namespace Vbodlaci.Web.Domain.Contacts;

public sealed class ContactInquiry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string SourcePage { get; set; } = string.Empty;

    public string ClientIp { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
