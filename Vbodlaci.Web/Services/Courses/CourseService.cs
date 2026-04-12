using Microsoft.EntityFrameworkCore;
using Vbodlaci.Web.Application.Common;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Application.Newsletter;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Services.Courses;

public sealed class CourseService(
    ApplicationDbContext dbContext,
    INewsletterService newsletterService) : ICourseService
{
    public async Task<IReadOnlyList<CourseListItem>> GetPublicCoursesAsync(CourseQueryFilter filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Courses
            .AsNoTracking()
            .Where(course => !course.IsDeleted)
            .Where(course => course.Status == CourseStatus.Published)
            .Where(course => course.StartDateTime >= DateTimeOffset.UtcNow);

        if (filter.Type.HasValue)
        {
            query = query.Where(course => course.Type == filter.Type.Value);
        }

        query = query.OrderBy(course => course.StartDateTime);

        if (filter.Take is > 0)
        {
            query = query.Take(filter.Take.Value);
        }

        var courses = await query.ToListAsync(cancellationToken);
        return courses.Select(MapToListItem).ToList();
    }

    public async Task<IReadOnlyList<CourseListItem>> GetAdminCoursesAsync(CancellationToken cancellationToken = default)
    {
        var courses = await dbContext.Courses
            .AsNoTracking()
            .Where(course => !course.IsDeleted)
            .OrderBy(course => course.StartDateTime)
            .ToListAsync(cancellationToken);

        return courses.Select(MapToListItem).ToList();
    }

    public async Task<CourseDetailViewModel?> GetPublicCourseBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses
            .AsNoTracking()
            .Where(item => !item.IsDeleted)
            .FirstOrDefaultAsync(item => item.Slug == slug, cancellationToken);

        if (course is null || course.Status == CourseStatus.Draft)
        {
            return null;
        }

        return MapToDetail(course);
    }

    public async Task<CourseDetailViewModel?> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses
            .AsNoTracking()
            .Where(item => !item.IsDeleted)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return course is null ? null : MapToDetail(course);
    }

    public async Task<(ServiceResult Result, Guid? Id)> CreateAsync(CourseEditModel model, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Type = model.Type,
            Status = model.Status,
            Title = model.Title.Trim(),
            StartDateTime = model.StartDateTime.ToUniversalTime(),
            EndDateTime = ToUniversalTime(model.EndDateTime),
            CityOrArea = model.CityOrArea.Trim(),
            VenueText = model.VenueText.Trim(),
            PriceText = model.PriceText.Trim(),
            CapacityInfo = model.CapacityInfo,
            RegistrationDeadline = ToUniversalTime(model.RegistrationDeadline),
            ShortDescription = model.ShortDescription.Trim(),
            FullDescription = model.FullDescription.Trim(),
            WhatToExpect = model.WhatToExpect.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
            PublishedAt = model.Status == CourseStatus.Published ? now : null
        };

        course.Slug = await BuildUniqueSlugAsync(course.Title, null, cancellationToken);

        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (course.Status == CourseStatus.Published)
        {
            await newsletterService.NotifyCoursePublishedAsync(course, cancellationToken);
        }

        return (ServiceResult.Success("Kurz byl vytvořen."), course.Id);
    }

    public async Task<ServiceResult> UpdateAsync(Guid id, CourseEditModel model, CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (course is null)
        {
            return ServiceResult.Failure("Kurz nebyl nalezen.");
        }

        var wasPublished = course.Status == CourseStatus.Published;

        course.Type = model.Type;
        course.Status = model.Status;
        course.Title = model.Title.Trim();
        course.StartDateTime = model.StartDateTime.ToUniversalTime();
        course.EndDateTime = ToUniversalTime(model.EndDateTime);
        course.CityOrArea = model.CityOrArea.Trim();
        course.VenueText = model.VenueText.Trim();
        course.PriceText = model.PriceText.Trim();
        course.CapacityInfo = model.CapacityInfo;
        course.RegistrationDeadline = ToUniversalTime(model.RegistrationDeadline);
        course.ShortDescription = model.ShortDescription.Trim();
        course.FullDescription = model.FullDescription.Trim();
        course.WhatToExpect = model.WhatToExpect.Trim();
        course.UpdatedAt = DateTimeOffset.UtcNow;
        course.Slug = await BuildUniqueSlugAsync(course.Title, course.Id, cancellationToken);

        if (!wasPublished && course.Status == CourseStatus.Published)
        {
            course.PublishedAt ??= DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (!wasPublished && course.Status == CourseStatus.Published)
        {
            await newsletterService.NotifyCoursePublishedAsync(course, cancellationToken);
        }

        return ServiceResult.Success("Kurz byl uložen.");
    }

    public async Task<ServiceResult> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (course is null)
        {
            return ServiceResult.Failure("Kurz nebyl nalezen.");
        }

        course.IsDeleted = true;
        course.Status = CourseStatus.Draft;
        course.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success("Kurz byl odstraněn.");
    }

    public async Task<ServiceResult> ChangeStatusAsync(Guid id, CourseStatus status, CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (course is null)
        {
            return ServiceResult.Failure("Kurz nebyl nalezen.");
        }

        var wasPublished = course.Status == CourseStatus.Published;
        course.Status = status;
        course.UpdatedAt = DateTimeOffset.UtcNow;
        if (!wasPublished && status == CourseStatus.Published)
        {
            course.PublishedAt ??= DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (!wasPublished && status == CourseStatus.Published)
        {
            await newsletterService.NotifyCoursePublishedAsync(course, cancellationToken);
        }

        return ServiceResult.Success("Stav kurzu byl změněn.");
    }

    private async Task<string> BuildUniqueSlugAsync(string title, Guid? excludedCourseId, CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Generate(title);
        var candidate = baseSlug;
        var index = 2;

        while (await dbContext.Courses
                   .AsNoTracking()
                   .AnyAsync(course => !course.IsDeleted && course.Slug == candidate && (!excludedCourseId.HasValue || course.Id != excludedCourseId.Value), cancellationToken))
        {
            candidate = $"{baseSlug}-{index}";
            index++;
        }

        return candidate;
    }

    private static CourseListItem MapToListItem(Course course)
    {
        return new CourseListItem
        {
            Id = course.Id,
            Type = course.Type,
            Status = course.Status,
            Title = course.Title,
            Slug = course.Slug,
            StartDateTime = course.StartDateTime,
            CityOrArea = course.CityOrArea,
            PriceText = course.PriceText,
            ShortDescription = course.ShortDescription
        };
    }

    private static CourseDetailViewModel MapToDetail(Course course)
    {
        return new CourseDetailViewModel
        {
            Id = course.Id,
            Type = course.Type,
            Status = course.Status,
            Title = course.Title,
            Slug = course.Slug,
            StartDateTime = course.StartDateTime,
            EndDateTime = course.EndDateTime,
            CityOrArea = course.CityOrArea,
            VenueText = course.VenueText,
            PriceText = course.PriceText,
            CapacityInfo = course.CapacityInfo,
            RegistrationDeadline = course.RegistrationDeadline,
            ShortDescription = course.ShortDescription,
            FullDescription = course.FullDescription,
            WhatToExpect = course.WhatToExpect,
            PublishedAt = course.PublishedAt,
            FacebookPostText = BuildFacebookText(course)
        };
    }

    private static string BuildFacebookText(Course course)
    {
        var localStart = course.StartDateTime.ToLocalTime();

        return $"{course.Title}\n" +
               $"Typ: {(course.Type == CourseType.Breathwork ? "Breathwork v bodláčí" : "Koně v bodláčí")}\n" +
               $"Termín: {localStart:dd.MM.yyyy HH:mm}\n" +
               $"Místo: {course.CityOrArea}\n" +
               $"Cena: {course.PriceText}\n\n" +
               "Přihlášení na webu V bodláčí.";
    }

    private static DateTimeOffset? ToUniversalTime(DateTimeOffset? value)
    {
        return value?.ToUniversalTime();
    }
}
