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
    IOptions<SiteOptions> siteOptions) : ICourseService
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
        if (model.Status is not (CourseStatus.Draft or CourseStatus.Published))
        {
            return (ServiceResult.Failure("Nový kurz lze uložit pouze jako draft nebo publikovat."), null);
        }

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
            VenueText = model.VenueText?.Trim() ?? string.Empty,
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

        if (course.Status == CourseStatus.Canceled)
        {
            return ServiceResult.Failure("Zrušený kurz je dostupný pouze pro čtení.");
        }

        if (model.Status != course.Status)
        {
            return ServiceResult.Failure("Stav kurzu nelze měnit přes uložení formuláře.");
        }

        course.Type = model.Type;
        course.Title = model.Title.Trim();
        course.StartDateTime = model.StartDateTime.ToUniversalTime();
        course.EndDateTime = ToUniversalTime(model.EndDateTime);
        course.CityOrArea = model.CityOrArea.Trim();
        course.VenueText = model.VenueText?.Trim() ?? string.Empty;
        course.PriceText = model.PriceText.Trim();
        course.CapacityInfo = model.CapacityInfo;
        course.RegistrationDeadline = ToUniversalTime(model.RegistrationDeadline);
        course.ShortDescription = model.ShortDescription.Trim();
        course.FullDescription = model.FullDescription.Trim();
        course.WhatToExpect = model.WhatToExpect.Trim();
        course.UpdatedAt = DateTimeOffset.UtcNow;
        course.Slug = await BuildUniqueSlugAsync(course.Title, course.Id, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success("Kurz byl uložen.");
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
            return ServiceResult.Failure("Smazat lze pouze kurz ve stavu draft.");
        }

        course.IsDeleted = true;
        course.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

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
            return ServiceResult.Failure("Publikovat lze pouze kurz ve stavu draft.");
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
        var localCourseStart = course.StartDateTime.ToLocalTime();
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
                $"Původní termín: {localCourseStart:dd.MM.yyyy HH:mm}\n" +
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
