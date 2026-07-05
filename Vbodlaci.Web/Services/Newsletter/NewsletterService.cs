using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vbodlaci.Web.Application.Common;
using Vbodlaci.Web.Application.Configuration;
using Vbodlaci.Web.Application.Newsletter;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Courses;
using Vbodlaci.Web.Domain.Newsletter;
using Vbodlaci.Web.Services.Email;

namespace Vbodlaci.Web.Services.Newsletter;

public sealed class NewsletterService(
    ApplicationDbContext dbContext,
    IEmailDispatcher emailDispatcher,
    IOptions<SiteOptions> siteOptions) : INewsletterService
{
    public async Task<ServiceResult> SubscribeAsync(NewsletterSubscribeInput input, string clientIp, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var normalizedEmail = input.Email.Trim().ToLowerInvariant();

        if (!input.PrefBreathwork && !input.PrefHorses && !input.PrefVeterinary)
        {
            input.PrefBreathwork = true;
            input.PrefHorses = true;
            input.PrefVeterinary = true;
        }

        var subscriber = await dbContext.NewsletterSubscribers.FirstOrDefaultAsync(item => item.Email == normalizedEmail, cancellationToken);
        if (subscriber is null)
        {
            subscriber = new NewsletterSubscriber
            {
                Email = normalizedEmail,
                PrefBreathwork = input.PrefBreathwork,
                PrefHorses = input.PrefHorses,
                PrefVeterinary = input.PrefVeterinary,
                IsSubscribed = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            dbContext.NewsletterSubscribers.Add(subscriber);
        }
        else
        {
            subscriber.PrefBreathwork = input.PrefBreathwork;
            subscriber.PrefHorses = input.PrefHorses;
            subscriber.PrefVeterinary = input.PrefVeterinary;
            subscriber.IsSubscribed = true;
            subscriber.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Děkujeme, odběr newsletteru je aktivní.");
    }

    public async Task<ServiceResult> UnsubscribeAsync(string token, CancellationToken cancellationToken = default)
    {
        var subscriber = await dbContext.NewsletterSubscribers.FirstOrDefaultAsync(item => item.UnsubscribeToken == token, cancellationToken);
        if (subscriber is null)
        {
            return ServiceResult.Failure("Odkaz pro odhlášení je neplatný.");
        }

        subscriber.IsSubscribed = false;
        subscriber.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success("Byl/a jsi úspěšně odhlášen/a z newsletteru.");
    }

    public async Task<int> NotifyCoursePublishedAsync(Course course, CancellationToken cancellationToken = default)
    {
        var subscribers = await dbContext.NewsletterSubscribers
            .Where(item => item.IsSubscribed)
            .ToListAsync(cancellationToken);

        var targetSubscribers = subscribers.Where(subscriber => MatchesPreference(subscriber, course.Type)).ToList();
        var sent = 0;

        foreach (var subscriber in targetSubscribers)
        {
            var wasSent = await dbContext.NewsletterDispatchLogs
                .AsNoTracking()
                .AnyAsync(item => item.CourseId == course.Id && item.SubscriberId == subscriber.Id, cancellationToken);
            if (wasSent)
            {
                continue;
            }

            var unsubscribeUrl = $"{siteOptions.Value.SiteUrl.TrimEnd('/')}/newsletter/odhlaseni/{subscriber.UnsubscribeToken}";
            var courseUrl = $"{siteOptions.Value.SiteUrl.TrimEnd('/')}/kurzy/{course.Slug}";
            var subject = $"Nový termín: {course.Title}";
            var body = $"Ahoj,\n\nprávě jsme vypsali nový kurz:\n" +
                       $"{course.Title}\n" +
                       $"Termín: {course.CourseDate:dd.MM.yyyy} {course.TimeText}\n" +
                       $"Místo: {course.CityOrArea}\n" +
                       $"Cena: {course.PriceText}\n\n" +
                       $"Detail a přihlášení: {courseUrl}\n\n" +
                       $"Odhlášení z newsletteru: {unsubscribeUrl}";

            var success = await emailDispatcher.SendAsync("NewsletterCoursePublished", subscriber.Email, subject, body, cancellationToken: cancellationToken);
            if (!success)
            {
                continue;
            }

            dbContext.NewsletterDispatchLogs.Add(new NewsletterDispatchLog
            {
                CourseId = course.Id,
                SubscriberId = subscriber.Id,
                SentAt = DateTimeOffset.UtcNow
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            sent++;
        }

        return sent;
    }

    public async Task<IReadOnlyList<NewsletterSubscriberItem>> GetSubscribersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.NewsletterSubscribers
            .AsNoTracking()
            .OrderBy(item => item.Email)
            .Select(item => new NewsletterSubscriberItem
            {
                Id = item.Id,
                Email = item.Email,
                PrefBreathwork = item.PrefBreathwork,
                PrefHorses = item.PrefHorses,
                PrefVeterinary = item.PrefVeterinary,
                IsSubscribed = item.IsSubscribed,
                CreatedAt = item.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<string> ExportSubscribersCsvAsync(CancellationToken cancellationToken = default)
    {
        var subscribers = await GetSubscribersAsync(cancellationToken);
        var lines = new List<string> { "Email;Breathwork;Kone;Veterina;Subscribed;CreatedAt" };
        lines.AddRange(subscribers.Select(item =>
            $"{item.Email};{item.PrefBreathwork};{item.PrefHorses};{item.PrefVeterinary};{item.IsSubscribed};{item.CreatedAt:O}"));

        return string.Join(Environment.NewLine, lines);
    }

    private static bool MatchesPreference(NewsletterSubscriber subscriber, CourseType type)
    {
        return type switch
        {
            CourseType.Breathwork => subscriber.PrefBreathwork,
            CourseType.Horses => subscriber.PrefHorses,
            _ => false
        };
    }
}

