namespace Vbodlaci.Web.Application.Contacts;

public sealed class ContactInboxItem
{
    public Guid Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string SourcePage { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public string ClientIp { get; init; } = string.Empty;
}
