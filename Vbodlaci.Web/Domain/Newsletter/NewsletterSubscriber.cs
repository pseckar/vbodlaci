namespace Vbodlaci.Web.Domain.Newsletter;

public sealed class NewsletterSubscriber
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;

    public bool PrefBreathwork { get; set; }

    public bool PrefKone { get; set; }

    public bool PrefVeterina { get; set; }

    public bool IsSubscribed { get; set; } = true;

    public string UnsubscribeToken { get; set; } = Guid.NewGuid().ToString("N");

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
