using Microsoft.Extensions.DependencyInjection;
using Vbodlaci.Web.Application.Contacts;
using Vbodlaci.Web.Application.Newsletter;
using Vbodlaci.Web.Application.Registrations;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Courses;
using Vbodlaci.Web.Tests.Infrastructure;

namespace Vbodlaci.Web.Tests;

public class FormFlowsTests
{
    [Fact]
    public async Task ContactFlow_PersistsInquiry_AndReturnsSuccess()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<IContactService>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var result = await service.SubmitAsync(new ContactMessageInput
        {
            FullName = "Test User",
            Email = "test@example.com",
            Message = "Ahoj, prosím o info.",
            SourcePage = "home",
            Honeypot = string.Empty
        }, "127.0.0.1");

        Assert.True(result.Succeeded);
        Assert.Single(db.ContactInquiries);
    }

    [Fact]
    public async Task NewsletterFlow_PersistsSubscriber_AndReturnsSuccess()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<INewsletterService>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var result = await service.SubscribeAsync(new NewsletterSubscribeInput
        {
            Email = "newsletter@example.com",
            PrefBreathwork = true,
            PrefKone = false,
            PrefVeterina = true,
            Honeypot = string.Empty
        }, "127.0.0.1");

        Assert.True(result.Succeeded);
        Assert.Single(db.NewsletterSubscribers);
    }

    [Fact]
    public async Task CourseRegistrationFlow_PersistsRegistration_AndReturnsSuccess()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IRegistrationService>();
        var now = DateTimeOffset.UtcNow;
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Type = CourseType.Breathwork,
            Status = CourseStatus.Published,
            Title = "Testovací kurz",
            Slug = "testovaci-kurz",
            StartDateTime = now.AddDays(7),
            EndDateTime = now.AddDays(7).AddHours(3),
            CityOrArea = "Hlinsko",
            VenueText = "Les",
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

        var result = await service.RegisterAsync(course.Id, new CourseRegistrationInput
        {
            FullName = "Uživatel",
            Email = "uzivatel@example.com",
            Note = "Poznámka",
            TermsConsent = true,
            Honeypot = string.Empty
        }, "127.0.0.1");

        Assert.True(result.Succeeded);
        Assert.Single(db.CourseRegistrations.Where(item => item.CourseId == course.Id));
    }
}

