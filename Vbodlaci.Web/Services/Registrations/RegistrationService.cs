using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vbodlaci.Web.Application.Common;
using Vbodlaci.Web.Application.Configuration;
using Vbodlaci.Web.Application.Registrations;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Courses;
using Vbodlaci.Web.Domain.Registrations;
using Vbodlaci.Web.Services.Email;

namespace Vbodlaci.Web.Services.Registrations;

public sealed class RegistrationService(
    ApplicationDbContext dbContext,
    IEmailDispatcher emailDispatcher,
    IOptions<SiteOptions> siteOptions) : IRegistrationService
{
    public async Task<ServiceResult> RegisterAsync(Guid courseId, CourseRegistrationInput input, string clientIp, CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == courseId && !item.IsDeleted, cancellationToken);

        if (course is null)
        {
            return ServiceResult.Failure("Kurz nebyl nalezen.");
        }

        if (course.Status != CourseStatus.Published)
        {
            return ServiceResult.Failure("Na tento kurz se teď nelze přihlásit.");
        }

        var now = DateTimeOffset.UtcNow;

        var registration = new CourseRegistration
        {
            CourseId = course.Id,
            FullName = input.FullName.Trim(),
            Email = input.Email.Trim(),
            Note = input.Note?.Trim() ?? string.Empty,
            TermsConsent = input.TermsConsent,
            ClientIp = clientIp,
            CreatedAt = now
        };

        dbContext.CourseRegistrations.Add(registration);
        await dbContext.SaveChangesAsync(cancellationToken);

        var adminSubject = $"Nová přihláška na kurz: {course.Title}";
        var localCourseStart = course.StartDateTime.ToLocalTime();
        var adminBody = $"Kurz: {course.Title}\n" +
                        $"Termín: {localCourseStart:dd.MM.yyyy HH:mm}\n" +
                        $"Jméno: {registration.FullName}\n" +
                        $"E-mail: {registration.Email}\n" +
                        $"Poznámka: {registration.Note}";

        await emailDispatcher.SendAsync(
            "CourseRegistrationAdmin",
            siteOptions.Value.RegistrationInboxEmail,
            adminSubject,
            adminBody,
            cancellationToken: cancellationToken);

        var userSubject = $"Potvrzení přihlášky: {course.Title}";
        var userBody = $"Ahoj {registration.FullName},\n\n" +
                       "děkujeme za přihlášení na kurz V bodláčí.\n\n" +
                       $"Kurz: {course.Title}\n" +
                       $"Termín: {localCourseStart:dd.MM.yyyy HH:mm}\n" +
                       $"Místo: {course.CityOrArea}\n" +
                       $"Cena: {course.PriceText}\n\n" +
                       "Brzy se ozvu s dalšími informacemi.\n\n" +
                       "Veronika";

        await emailDispatcher.SendAsync(
            "CourseRegistrationUser",
            registration.Email,
            userSubject,
            userBody,
            cancellationToken: cancellationToken);

        return ServiceResult.Success("Přihláška byla odeslána. Brzy se ozvu e-mailem.");
    }
}
