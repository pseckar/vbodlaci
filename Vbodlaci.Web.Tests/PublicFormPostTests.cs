using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Courses;
using Vbodlaci.Web.Tests.Infrastructure;

namespace Vbodlaci.Web.Tests;

public class PublicFormPostTests
{
    [Fact]
    public async Task ContactPost_WithEmptyHoneypot_PersistsInquiry()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/akce/kontakt", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["ReturnUrl"] = "/",
            ["SourcePage"] = "home",
            ["Honeypot"] = string.Empty,
            ["FullName"] = "Test User",
            ["Email"] = "test@example.com",
            ["Message"] = "Ahoj, test dotaz."
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Single(db.ContactInquiries);
    }

    [Fact]
    public async Task NewsletterPost_WithEmptyHoneypot_PersistsSubscriberWithSelectedPreferences()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        // browsers omit unchecked checkboxes entirely; checked ones post value="true"
        var response = await client.PostAsync("/akce/newsletter", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["ReturnUrl"] = "/",
            ["Honeypot"] = string.Empty,
            ["Email"] = "newsletter@example.com",
            ["PrefHorses"] = "true"
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var subscriber = Assert.Single(db.NewsletterSubscribers);
        Assert.False(subscriber.PrefBreathwork);
        Assert.True(subscriber.PrefHorses);
        Assert.False(subscriber.PrefVeterinary);
    }

    [Fact]
    public async Task CourseRegistrationPost_WithEmptyNoteAndHoneypot_PersistsRegistration()
    {
        await using var factory = new TestWebApplicationFactory();

        Guid courseId;
        const string slug = "test-post-registrace";

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTimeOffset.UtcNow;

            var course = new Course
            {
                Id = Guid.NewGuid(),
                Type = CourseType.Breathwork,
                Status = CourseStatus.Published,
                Title = "Testovací post kurz",
                Slug = slug,
                CourseDate = DateOnly.FromDateTime(now.AddDays(7).DateTime),
                TimeText = "18:00-21:00",
                CityOrArea = "Hlinsko",
                PriceText = "1 900 Kč",
                ShortDescription = "Krátký popis",
                FullDescription = "Detailní popis",
                WhatToExpect = "Co tě čeká",
                CreatedAt = now,
                UpdatedAt = now,
                PublishedAt = now
            };

            db.Courses.Add(course);
            await db.SaveChangesAsync();
            courseId = course.Id;
        }

        using var client = factory.CreateClient();

        var response = await client.PostAsync($"/kurzy/{slug}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Registration.Honeypot"] = string.Empty,
            ["Registration.FullName"] = "Registrace Test",
            ["Registration.Email"] = "registrace@example.com",
            ["Registration.Note"] = string.Empty,
            ["Registration.TermsConsent"] = "true"
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Single(verifyDb.CourseRegistrations.Where(item => item.CourseId == courseId));
    }

    [Fact]
    public async Task CourseRegistrationPost_WithUncheckedTerms_DoesNotPersistRegistration()
    {
        await using var factory = new TestWebApplicationFactory();

        Guid courseId;
        const string slug = "test-post-registrace-bez-souhlasu";

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTimeOffset.UtcNow;

            var course = new Course
            {
                Id = Guid.NewGuid(),
                Type = CourseType.Breathwork,
                Status = CourseStatus.Published,
                Title = "Testovací kurz bez souhlasu",
                Slug = slug,
                CourseDate = DateOnly.FromDateTime(now.AddDays(7).DateTime),
                TimeText = "18:00-21:00",
                CityOrArea = "Hlinsko",
                PriceText = "1 900 Kč",
                ShortDescription = "Krátký popis",
                FullDescription = "Detailní popis",
                WhatToExpect = "Co tě čeká",
                CreatedAt = now,
                UpdatedAt = now,
                PublishedAt = now
            };

            db.Courses.Add(course);
            await db.SaveChangesAsync();
            courseId = course.Id;
        }

        using var client = factory.CreateClient();

        var response = await client.PostAsync($"/kurzy/{slug}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Registration.Honeypot"] = string.Empty,
            ["Registration.FullName"] = "Registrace Test",
            ["Registration.Email"] = "registrace@example.com",
            ["Registration.Note"] = string.Empty
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Empty(verifyDb.CourseRegistrations.Where(item => item.CourseId == courseId));
    }
}
