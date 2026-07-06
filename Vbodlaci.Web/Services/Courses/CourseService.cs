using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vbodlaci.Web.Application.Common;
using Vbodlaci.Web.Application.Configuration;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Application.Newsletter;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Courses;
using Vbodlaci.Web.Services.Email;

namespace Vbodlaci.Web.Services.Courses;

public sealed class CourseService(
    ApplicationDbContext dbContext,
    INewsletterService newsletterService,
    IEmailDispatcher emailDispatcher,
    IOptions<SiteOptions> siteOptions,
    ICourseImageService courseImageService) : ICourseService
{
    public async Task<IReadOnlyList<CourseListItem>> GetPublicCoursesAsync(CourseQueryFilter filter, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = dbContext.Courses
            .AsNoTracking()
            .Where(course => !course.IsDeleted)
            .Where(course => course.Status == CourseStatus.Published)
            .Where(course => course.CourseDate >= today);

        if (filter.Type.HasValue)
        {
            query = query.Where(course => course.Type == filter.Type.Value);
        }

        query = query.OrderBy(course => course.CourseDate).ThenBy(course => course.Title);

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
            .OrderBy(course => course.CourseDate)
            .ThenBy(course => course.Title)
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

    public async Task<IReadOnlyList<CourseTextDefaultItem>> GetTextDefaultsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.CourseTextDefaults
            .AsNoTracking()
            .OrderBy(item => item.Type)
            .ThenBy(item => item.Field)
            .Select(item => new CourseTextDefaultItem
            {
                Type = item.Type,
                Field = item.Field,
                Text = item.Text
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult> UpdateTextDefaultAsync(CourseType type, CourseTextField field, string text, CancellationToken cancellationToken = default)
    {
        var defaultText = await dbContext.CourseTextDefaults
            .FirstOrDefaultAsync(item => item.Type == type && item.Field == field, cancellationToken);

        if (defaultText is null)
        {
            defaultText = new CourseTextDefault
            {
                Type = type,
                Field = field
            };
            dbContext.CourseTextDefaults.Add(defaultText);
        }

        defaultText.Text = string.IsNullOrWhiteSpace(text) ? "This is placeholder for default text" : text.Trim();
        defaultText.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Výchozí text byl uložen.");
    }

    public async Task<(ServiceResult Result, Guid? Id)> CreateAsync(CourseEditModel model, CancellationToken cancellationToken = default)
    {
        if (model.Status is not (CourseStatus.Draft or CourseStatus.Published))
        {
            return (ServiceResult.Failure("Nový kurz lze uložit pouze jako návrh nebo publikovat."), null);
        }

        var imagePath = string.Empty;
        var thumbnailPath = string.Empty;
        string? imageWarning = null;

        if (model.Image is not null)
        {
            var (imageResult, image) = await courseImageService.SaveAsync(model.Image, cancellationToken);
            if (!imageResult.Succeeded || image is null)
            {
                return (imageResult, null);
            }

            imagePath = image.ImagePath;
            thumbnailPath = image.ThumbnailPath;
            imageWarning = image.Warning;
        }

        var now = DateTimeOffset.UtcNow;
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Type = model.Type,
            Status = model.Status,
            Title = model.Title.Trim(),
            CourseDate = model.CourseDate,
            TimeText = model.TimeText.Trim(),
            CityOrArea = model.CityOrArea.Trim(),
            PriceText = model.PriceText.Trim(),
            CapacityInfo = model.CapacityInfo,
            ShortDescription = model.ShortDescription.Trim(),
            FullDescription = model.FullDescription.Trim(),
            IsFullDescriptionVisible = model.IsFullDescriptionVisible,
            WhatToExpect = model.WhatToExpect.Trim(),
            IsWhatToExpectVisible = model.IsWhatToExpectVisible,
            ImagePath = imagePath,
            ThumbnailPath = thumbnailPath,
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

        var message = imageWarning is null
            ? "Kurz byl vytvořen."
            : $"Kurz byl vytvořen. {imageWarning}";

        return (ServiceResult.Success(message), course.Id);
    }

    public async Task<ServiceResult> UpdateAsync(Guid id, CourseEditModel model, CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (course is null)
        {
            return ServiceResult.Failure("Kurz nebyl nalezen.");
        }

        if (course.Status == CourseStatus.Canceled)
        {
            return ServiceResult.Failure("Zrušený kurz je dostupný pouze pro čtení.");
        }

        if (model.Status != course.Status)
        {
            return ServiceResult.Failure("Stav kurzu nelze měnit přes uložení formuláře.");
        }

        var oldImagePath = course.ImagePath;
        var oldThumbnailPath = course.ThumbnailPath;
        string? imageWarning = null;

        if (model.Image is not null)
        {
            var (imageResult, image) = await courseImageService.SaveAsync(model.Image, cancellationToken);
            if (!imageResult.Succeeded || image is null)
            {
                return imageResult;
            }

            course.ImagePath = image.ImagePath;
            course.ThumbnailPath = image.ThumbnailPath;
            imageWarning = image.Warning;
        }

        course.Type = model.Type;
        course.Title = model.Title.Trim();
        course.CourseDate = model.CourseDate;
        course.TimeText = model.TimeText.Trim();
        course.CityOrArea = model.CityOrArea.Trim();
        course.PriceText = model.PriceText.Trim();
        course.CapacityInfo = model.CapacityInfo;
        course.ShortDescription = model.ShortDescription.Trim();
        course.FullDescription = model.FullDescription.Trim();
        course.IsFullDescriptionVisible = model.IsFullDescriptionVisible;
        course.WhatToExpect = model.WhatToExpect.Trim();
        course.IsWhatToExpectVisible = model.IsWhatToExpectVisible;
        course.UpdatedAt = DateTimeOffset.UtcNow;
        course.Slug = await BuildUniqueSlugAsync(course.Title, course.Id, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        if (model.Image is not null)
        {
            courseImageService.DeleteCourseImages(oldImagePath, oldThumbnailPath);
        }

        return ServiceResult.Success(imageWarning is null
            ? "Kurz byl uložen."
            : $"Kurz byl uložen. {imageWarning}");
    }

    public async Task<ServiceResult> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (course is null)
        {
            return ServiceResult.Failure("Kurz nebyl nalezen.");
        }

        if (course.Status != CourseStatus.Draft)
        {
            return ServiceResult.Failure("Smazat lze pouze návrh kurzu.");
        }

        course.IsDeleted = true;
        course.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        courseImageService.DeleteCourseImages(course.ImagePath, course.ThumbnailPath);

        return ServiceResult.Success("Kurz byl smazán.");
    }

    public async Task<ServiceResult> ChangeStatusAsync(Guid id, CourseStatus status, CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (course is null)
        {
            return ServiceResult.Failure("Kurz nebyl nalezen.");
        }

        if (course.Status == CourseStatus.Canceled)
        {
            return ServiceResult.Failure("Zrušený kurz je dostupný pouze pro čtení.");
        }

        switch (status)
        {
            case CourseStatus.Published:
                return await PublishAsync(course, cancellationToken);
            case CourseStatus.Canceled:
                return await CancelAsync(course, cancellationToken);
            default:
                return ServiceResult.Failure("Požadovaná změna stavu není v MVP podporovaná.");
        }
    }

    private async Task<ServiceResult> PublishAsync(Course course, CancellationToken cancellationToken)
    {
        if (course.Status != CourseStatus.Draft)
        {
            return ServiceResult.Failure("Publikovat lze pouze návrh kurzu.");
        }

        course.Status = CourseStatus.Published;
        course.PublishedAt ??= DateTimeOffset.UtcNow;
        course.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await newsletterService.NotifyCoursePublishedAsync(course, cancellationToken);

        return ServiceResult.Success("Kurz byl publikován.");
    }

    private async Task<ServiceResult> CancelAsync(Course course, CancellationToken cancellationToken)
    {
        if (course.Status != CourseStatus.Published)
        {
            return ServiceResult.Failure("Zrušit lze pouze publikovaný kurz.");
        }

        course.Status = CourseStatus.Canceled;
        course.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var sentCount = await NotifyParticipantsAboutCancellationAsync(course, cancellationToken);
        return sentCount > 0
            ? ServiceResult.Success($"Kurz byl zrušen. Odesláno {sentCount} e-mailů účastníkům.")
            : ServiceResult.Success("Kurz byl zrušen. Kurz zatím neměl žádné účastníky.");
    }

    private async Task<int> NotifyParticipantsAboutCancellationAsync(Course course, CancellationToken cancellationToken)
    {
        var participants = await dbContext.CourseRegistrations
            .AsNoTracking()
            .Where(item => item.CourseId == course.Id)
            .Select(item => new
            {
                item.Email,
                item.FullName
            })
            .ToListAsync(cancellationToken);

        var uniqueParticipants = participants
            .Select(item => new
            {
                Email = item.Email.Trim(),
                FullName = item.FullName.Trim()
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.Email))
            .DistinctBy(item => item.Email, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (uniqueParticipants.Count == 0)
        {
            return 0;
        }

        var sent = 0;
        var courseUrl = $"{siteOptions.Value.SiteUrl.TrimEnd('/')}/kurzy/{course.Slug}";
        var subject = $"Zrušení kurzu: {course.Title}";

        foreach (var participant in uniqueParticipants)
        {
            var greeting = string.IsNullOrWhiteSpace(participant.FullName)
                ? "Ahoj,"
                : $"Ahoj {participant.FullName},";

            var body =
                $"{greeting}\n\n" +
                "omlouváme se, ale kurz byl zrušen.\n\n" +
                $"Kurz: {course.Title}\n" +
                $"Původní termín: {FormatDate(course.CourseDate)} {course.TimeText}\n" +
                $"Místo: {course.CityOrArea}\n" +
                $"Detail kurzu: {courseUrl}\n\n" +
                "Veronika";

            var success = await emailDispatcher.SendAsync(
                "CourseCanceledParticipant",
                participant.Email,
                subject,
                body,
                cancellationToken: cancellationToken);

            if (success)
            {
                sent++;
            }
        }

        return sent;
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
            CourseDate = course.CourseDate,
            TimeText = course.TimeText,
            CityOrArea = course.CityOrArea,
            PriceText = course.PriceText,
            ShortDescription = course.ShortDescription,
            ThumbnailImageUrl = ResolveImagePath(course.ThumbnailPath, CourseImageDefaults.DefaultThumbnailPath)
        };
    }

    private CourseDetailViewModel MapToDetail(Course course)
    {
        return new CourseDetailViewModel
        {
            Id = course.Id,
            Type = course.Type,
            Status = course.Status,
            Title = course.Title,
            Slug = course.Slug,
            CourseDate = course.CourseDate,
            TimeText = course.TimeText,
            CityOrArea = course.CityOrArea,
            PriceText = course.PriceText,
            CapacityInfo = course.CapacityInfo,
            ShortDescription = course.ShortDescription,
            FullDescription = course.FullDescription,
            IsFullDescriptionVisible = course.IsFullDescriptionVisible,
            WhatToExpect = course.WhatToExpect,
            IsWhatToExpectVisible = course.IsWhatToExpectVisible,
            ImageUrl = ResolveImagePath(course.ImagePath, CourseImageDefaults.DefaultImagePath),
            ThumbnailImageUrl = ResolveImagePath(course.ThumbnailPath, CourseImageDefaults.DefaultThumbnailPath),
            PublishedAt = course.PublishedAt,
            FacebookPostText = BuildFacebookText(course)
        };
    }

    private string BuildFacebookText(Course course)
    {
        var courseUrl = $"{siteOptions.Value.SiteUrl.TrimEnd('/')}/kurzy/{course.Slug}";

        return $"{course.Title}\n" +
               $"Typ: {(course.Type == CourseType.Breathwork ? "Breathwork v bodláčí" : "Koně v bodláčí")}\n" +
               $"Termín: {FormatDate(course.CourseDate)} {course.TimeText}\n" +
               $"Místo: {course.CityOrArea}\n" +
               $"Cena: {course.PriceText}\n\n" +
               $"Více informací a přihlášení: {courseUrl}";
    }

    private static string ResolveImagePath(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string FormatDate(DateOnly date)
    {
        return date.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.GetCultureInfo("cs-CZ"));
    }
}
