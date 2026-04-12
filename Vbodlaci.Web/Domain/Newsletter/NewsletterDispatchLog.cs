namespace Vbodlaci.Web.Domain.Newsletter;

public sealed class NewsletterDispatchLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CourseId { get; set; }

    public Guid SubscriberId { get; set; }

    public DateTimeOffset SentAt { get; set; }
}
