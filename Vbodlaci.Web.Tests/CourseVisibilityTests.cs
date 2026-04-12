using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Courses;
using Vbodlaci.Web.Tests.Infrastructure;

namespace Vbodlaci.Web.Tests;

public class CourseVisibilityTests
{
    [Fact]
    public async Task DraftDelete_RemovesCourseFromAdminAndPublicQueries()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<ICourseService>();

        var draft = CreateCourse(CourseStatus.Draft, "draft-kurz");
        db.Courses.Add(draft);
        await db.SaveChangesAsync();

        var deleteResult = await service.SoftDeleteAsync(draft.Id);

        Assert.True(deleteResult.Succeeded);

        var stored = await db.Courses.AsNoTracking().SingleAsync(item => item.Id == draft.Id);
        Assert.True(stored.IsDeleted);

        var adminCourses = await service.GetAdminCoursesAsync();
        var publicCourses = await service.GetPublicCoursesAsync(new CourseQueryFilter { Take = 20 });

        Assert.DoesNotContain(adminCourses, item => item.Id == draft.Id);
        Assert.DoesNotContain(publicCourses, item => item.Id == draft.Id);
    }

    [Fact]
    public async Task PublishedCancel_HidesCourseFromPublicList_ButKeepsSlugAndAdminVisibility()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<ICourseService>();

        var published = CreateCourse(CourseStatus.Published, "zruseny-kurz");
        db.Courses.Add(published);
        await db.SaveChangesAsync();

        var cancelResult = await service.ChangeStatusAsync(published.Id, CourseStatus.Canceled);

        Assert.True(cancelResult.Succeeded);

        var stored = await db.Courses.AsNoTracking().SingleAsync(item => item.Id == published.Id);
        Assert.False(stored.IsDeleted);
        Assert.Equal(CourseStatus.Canceled, stored.Status);

        var publicList = await service.GetPublicCoursesAsync(new CourseQueryFilter { Take = 20 });
        var detail = await service.GetPublicCourseBySlugAsync(published.Slug);
        var adminList = await service.GetAdminCoursesAsync();

        Assert.DoesNotContain(publicList, item => item.Slug == published.Slug);
        Assert.NotNull(detail);
        Assert.Equal(CourseStatus.Canceled, detail!.Status);
        Assert.Contains(adminList, item => item.Id == published.Id && item.Status == CourseStatus.Canceled);
    }

    [Fact]
    public async Task CanceledCourse_RejectsFurtherStatusChangeDeleteAndUpdate()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<ICourseService>();

        var published = CreateCourse(CourseStatus.Published, "zruseny-bez-akci");
        db.Courses.Add(published);
        await db.SaveChangesAsync();

        var cancelResult = await service.ChangeStatusAsync(published.Id, CourseStatus.Canceled);
        Assert.True(cancelResult.Succeeded);

        var changeBackResult = await service.ChangeStatusAsync(published.Id, CourseStatus.Published);
        var deleteResult = await service.SoftDeleteAsync(published.Id);
        var updateResult = await service.UpdateAsync(published.Id, new CourseEditModel
        {
            Id = published.Id,
            Type = published.Type,
            Status = CourseStatus.Canceled,
            Title = published.Title,
            StartDateTime = published.StartDateTime,
            EndDateTime = published.EndDateTime,
            CityOrArea = published.CityOrArea,
            VenueText = published.VenueText,
            PriceText = published.PriceText,
            CapacityInfo = published.CapacityInfo,
            RegistrationDeadline = published.RegistrationDeadline,
            ShortDescription = published.ShortDescription,
            FullDescription = published.FullDescription,
            WhatToExpect = published.WhatToExpect
        });

        Assert.False(changeBackResult.Succeeded);
        Assert.False(deleteResult.Succeeded);
        Assert.False(updateResult.Succeeded);
    }

    private static Course CreateCourse(CourseStatus status, string slug)
    {
        var now = DateTimeOffset.UtcNow;

        return new Course
        {
            Id = Guid.NewGuid(),
            Type = CourseType.Horses,
            Status = status,
            Title = $"Kurz {slug}",
            Slug = slug,
            StartDateTime = now.AddDays(4),
            EndDateTime = now.AddDays(4).AddHours(2),
            CityOrArea = "Vysočina",
            VenueText = "Pastvina",
            PriceText = "2 000 Kč",
            ShortDescription = "Krátce",
            FullDescription = "Dlouze",
            WhatToExpect = "Obsah",
            CreatedAt = now,
            UpdatedAt = now,
            PublishedAt = status == CourseStatus.Published ? now : null
        };
    }
}
