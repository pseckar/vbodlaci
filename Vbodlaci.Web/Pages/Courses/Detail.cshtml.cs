using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Application.Registrations;
using Vbodlaci.Web.Application.Security;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Pages.Courses;

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

    public string SourceContext { get; private set; } = string.Empty;

    public string BackLinkText { get; private set; } = "← Zpět na hlavní stránku";

    public string BackLinkPage { get; private set; } = "/Index";

    public async Task<IActionResult> OnGetAsync(string slug, string? from, CancellationToken cancellationToken)
    {
        ResolveBackNavigation(from);

        if (!await LoadAsync(slug, cancellationToken))
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug, string? from, CancellationToken cancellationToken)
    {
        ResolveBackNavigation(from);

        if (!await LoadAsync(slug, cancellationToken))
        {
            return NotFound();
        }

        if (IsCanceled)
        {
            TempData["FlashMessage"] = "Kurz byl zrušen, přihlášení není možné.";
            TempData["FlashType"] = "error";
            return RedirectToPage(BuildRouteValues(slug));
        }

        if (!string.IsNullOrWhiteSpace(Registration.Honeypot))
        {
            TempData["FlashMessage"] = "Přihlášku se nepodařilo odeslat.";
            TempData["FlashType"] = "error";
            return RedirectToPage(BuildRouteValues(slug));
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!rateLimitService.IsAllowed($"registration:{ip}", 6, TimeSpan.FromMinutes(10)))
        {
            TempData["FlashMessage"] = "Posíláš příliš mnoho požadavků. Zkus to prosím za chvíli.";
            TempData["FlashType"] = "error";
            return RedirectToPage(BuildRouteValues(slug));
        }

        Registration.TermsConsent = IsConsentCheckedFromForm();
        if (!Registration.TermsConsent)
        {
            ModelState.AddModelError("Registration.TermsConsent", "Je potřeba souhlasit s podmínkami kurzu.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await registrationService.RegisterAsync(Course!.Id, Registration, ip, cancellationToken);
        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";

        return RedirectToPage(BuildRouteValues(slug));
    }

    private bool IsConsentCheckedFromForm()
    {
        var values = Request.Form["Registration.TermsConsent"];
        foreach (var value in values)
        {
            if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "on", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private object BuildRouteValues(string slug)
    {
        return string.IsNullOrWhiteSpace(SourceContext)
            ? new { slug }
            : new { slug, from = SourceContext };
    }

    private void ResolveBackNavigation(string? from)
    {
        SourceContext = (from ?? string.Empty).Trim().ToLowerInvariant();

        switch (SourceContext)
        {
            case "breathwork":
                BackLinkPage = "/Services/Breathwork";
                BackLinkText = "← Zpět na Breathwork";
                break;
            case "kone":
                BackLinkPage = "/Services/Horses";
                BackLinkText = "← Zpět na Koně";
                break;
            default:
                SourceContext = string.Empty;
                BackLinkPage = "/Index";
                BackLinkText = "← Zpět na hlavní stránku";
                break;
        }
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
            Type = Course.Type
        }, cancellationToken);

        RelatedCourses = related.Where(item => item.Id != Course.Id).ToList();
        return true;
    }
}
