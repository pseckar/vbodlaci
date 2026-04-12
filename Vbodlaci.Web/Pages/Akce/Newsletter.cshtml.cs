using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Newsletter;
using Vbodlaci.Web.Application.Security;

namespace Vbodlaci.Web.Pages.Akce;

public sealed class NewsletterModel(
    INewsletterService newsletterService,
    IRateLimitService rateLimitService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Vyplň e-mail.")]
    [EmailAddress(ErrorMessage = "Vyplň platný e-mail.")]
    [StringLength(180)]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public bool PrefBreathwork { get; set; }

    [BindProperty]
    public bool PrefKone { get; set; }

    [BindProperty]
    public bool PrefVeterina { get; set; }

    [BindProperty]
    public string ReturnUrl { get; set; } = "/";

    [BindProperty]
    public string Honeypot { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var safeReturn = Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : Url.Page("/Index")!;

        if (!string.IsNullOrWhiteSpace(Honeypot))
        {
            TempData["FlashMessage"] = "Přihlášení se nepodařilo odeslat.";
            TempData["FlashType"] = "error";
            return LocalRedirect(safeReturn);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!rateLimitService.IsAllowed($"newsletter:{ip}", 8, TimeSpan.FromMinutes(10)))
        {
            TempData["FlashMessage"] = "Posíláš příliš mnoho požadavků. Zkus to prosím za chvíli.";
            TempData["FlashType"] = "error";
            return LocalRedirect(safeReturn);
        }

        if (!ModelState.IsValid)
        {
            TempData["FlashMessage"] = "Prosím zkontroluj zadaný e-mail.";
            TempData["FlashType"] = "error";
            return LocalRedirect(safeReturn);
        }

        var result = await newsletterService.SubscribeAsync(new NewsletterSubscribeInput
        {
            Email = Email,
            PrefBreathwork = PrefBreathwork,
            PrefKone = PrefKone,
            PrefVeterina = PrefVeterina,
            Honeypot = Honeypot
        }, ip, cancellationToken);

        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";
        return LocalRedirect(safeReturn);
    }
}
