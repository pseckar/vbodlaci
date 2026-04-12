namespace Vbodlaci.Web.Application.Newsletter;

public sealed class NewsletterSubscriberItem
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public bool PrefBreathwork { get; init; }

    public bool PrefHorses { get; init; }

    public bool PrefVeterinary { get; init; }

    public bool IsSubscribed { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}

