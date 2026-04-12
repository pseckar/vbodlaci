using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Application.Registrations;
using Vbodlaci.Web.Application.Security;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Pages.Kurzy;

public sealed class DetailModel(
    ICourseService courseService,
    IRegistrationService registrationService,
    IRateLimitService rateLimitService) : PageModel
{
    [BindProperty]
    public CourseRegistrationInput Registration { get; set; } = new();

    public CourseDetailViewModel? Course { get; private set; }

    public IReadOnlyList<CourseListItem> RelatedCourses { get; private set; } = [];

    public bool IsCanceled => Course?.Status == CourseStatus.Canceled;

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken cancellationToken)
    {
        if (!await LoadAsync(slug, cancellationToken))
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug, CancellationToken cancellationToken)
    {
        if (!await LoadAsync(slug, cancellationToken))
        {
            return NotFound();
        }

        if (IsCanceled)
        {
            TempData["FlashMessage"] = "Kurz byl zrušen, přihlášení není možné.";
            TempData["FlashType"] = "error";
            return RedirectToPage(new { slug });
        }

        if (!string.IsNullOrWhiteSpace(Registration.Honeypot))
        {
            TempData["FlashMessage"] = "Přihlášku se nepodařilo odeslat.";
            TempData["FlashType"] = "error";
            return RedirectToPage(new { slug });
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!rateLimitService.IsAllowed($"registration:{ip}", 6, TimeSpan.FromMinutes(10)))
        {
            TempData["FlashMessage"] = "Posíláš příliš mnoho požadavků. Zkus to prosím za chvíli.";
            TempData["FlashType"] = "error";
            return RedirectToPage(new { slug });
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await registrationService.RegisterAsync(Course!.Id, Registration, ip, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";

        return RedirectToPage(new { slug });
    }

    private async Task<bool> LoadAsync(string slug, CancellationToken cancellationToken)
    {
        Course = await courseService.GetPublicCourseBySlugAsync(slug, cancellationToken);
        if (Course is null)
        {
            return false;
        }

        var related = await courseService.GetPublicCoursesAsync(new CourseQueryFilter
        {
            Type = Course.Type,
            Take = 8
        }, cancellationToken);

        RelatedCourses = related.Where(item => item.Id != Course.Id).ToList();
        return true;
    }
}
