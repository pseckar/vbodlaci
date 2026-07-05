using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Courses;
using Vbodlaci.Web.Tests.Infrastructure;

namespace Vbodlaci.Web.Tests;

public class NavigationRenderTests
{
    [Fact]
    public async Task HomePage_ContainsClientSideCourseFilterControls()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("data-course-filter-root", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("data-course-filter=\"all\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("data-course-filter=\"breathwork\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("data-course-filter=\"kone\"", html, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("breathwork", "Zpět na Breathwork", "/breathwork-v-bodlaci")]
    [InlineData("kone", "Zpět na Koně", "/kone-v-bodlaci")]
    public async Task CourseDetail_WithSourceContext_RendersCorrectBackLink(string from, string expectedLabel, string expectedHref)
    {
        await using var factory = new TestWebApplicationFactory();

        const string slug = "kurz-navigation-test";

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTimeOffset.UtcNow;

            db.Courses.Add(new Course
            {
                Id = Guid.NewGuid(),
                Type = CourseType.Breathwork,
                Status = CourseStatus.Published,
                Title = "Kurz navigace",
                Slug = slug,
                CourseDate = DateOnly.FromDateTime(now.AddDays(10).DateTime),
                TimeText = "18:00-20:00",
                CityOrArea = "Hlinsko",
                PriceText = "2 200 Kč",
                ShortDescription = "Krátce",
                FullDescription = "Detail",
                WhatToExpect = "Obsah",
                CreatedAt = now,
                UpdatedAt = now,
                PublishedAt = now
            });

            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/kurzy/{slug}?from={from}");
        var html = await response.Content.ReadAsStringAsync();
        var decodedHtml = WebUtility.HtmlDecode(html);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expectedLabel, decodedHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"href=\"{expectedHref}\"", html, StringComparison.OrdinalIgnoreCase);
    }
}
