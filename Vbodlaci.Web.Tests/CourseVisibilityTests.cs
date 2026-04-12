using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Application.Newsletter;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Courses;
using Vbodlaci.Web.Services.Courses;
using Vbodlaci.Web.Tests.Infrastructure;

namespace Vbodlaci.Web.Tests;

public class CourseVisibilityTests
{
    [Fact]
    public async Task CanceledCourse_IsHiddenFromPublicList_ButAvailableBySlug()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var now = DateTimeOffset.UtcNow;
        var canceled = new Course
        {
            Id = Guid.NewGuid(),
            Type = CourseType.Kone,
            Status = CourseStatus.Canceled,
            Title = "Zrušený kurz",
            Slug = "zruseny-kurz",
            StartDateTime = now.AddDays(4),
            CityOrArea = "Vysočina",
            VenueText = "Pastvina",
            PriceText = "2 000 Kč",
            ShortDescription = "Krátce",
            FullDescription = "Dlouze",
            WhatToExpect = "Obsah",
            CreatedAt = now,
            UpdatedAt = now,
            PublishedAt = now
        };
        db.Courses.Add(canceled);
        await db.SaveChangesAsync();

        var service = new CourseService(db, new FakeNewsletterService());
        var publicList = await service.GetPublicCoursesAsync(new CourseQueryFilter { Take = 20 });
        var detail = await service.GetPublicCourseBySlugAsync(canceled.Slug);

        Assert.DoesNotContain(publicList, item => item.Slug == canceled.Slug);
        Assert.NotNull(detail);
        Assert.Equal(CourseStatus.Canceled, detail!.Status);
    }

    private sealed class FakeNewsletterService : INewsletterService
    {
        public Task<Vbodlaci.Web.Application.Common.ServiceResult> SubscribeAsync(NewsletterSubscribeInput input, string clientIp, CancellationToken cancellationToken = default)
            => Task.FromResult(Vbodlaci.Web.Application.Common.ServiceResult.Success("ok"));

        public Task<Vbodlaci.Web.Application.Common.ServiceResult> UnsubscribeAsync(string token, CancellationToken cancellationToken = default)
            => Task.FromResult(Vbodlaci.Web.Application.Common.ServiceResult.Success("ok"));

        public Task<int> NotifyCoursePublishedAsync(Course course, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<IReadOnlyList<NewsletterSubscriberItem>> GetSubscribersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<NewsletterSubscriberItem>>([]);

        public Task<string> ExportSubscribersCsvAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(string.Empty);
    }
}
