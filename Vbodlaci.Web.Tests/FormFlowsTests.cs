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
            Message = "Ahoj, prosÃ­m o info.",
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
            PrefHorses = false,
            PrefVeterinary = true,
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
            Title = "TestovacÃ­ kurz",
            Slug = "testovaci-kurz",
            CourseDate = DateOnly.FromDateTime(now.AddDays(7).DateTime),
            TimeText = "18:00-21:00",
            CityOrArea = "Hlinsko",
            PriceText = "1 900 KÄ",
            ShortDescription = "KrÃ¡tkÃ½ popis",
            FullDescription = "DetailnÃ­ popis",
            WhatToExpect = "Co tÄ› ÄekÃ¡",
            CreatedAt = now,
            UpdatedAt = now,
            PublishedAt = now
        };

        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var result = await service.RegisterAsync(course.Id, new CourseRegistrationInput
        {
            FullName = "UÅ¾ivatel",
            Email = "uzivatel@example.com",
            Note = "PoznÃ¡mka",
            TermsConsent = true,
            Honeypot = string.Empty
        }, "127.0.0.1");

        Assert.True(result.Succeeded);
        Assert.Single(db.CourseRegistrations.Where(item => item.CourseId == course.Id));
    }
}


