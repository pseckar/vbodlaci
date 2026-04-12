using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Contacts;
using Vbodlaci.Web.Application.Security;

namespace Vbodlaci.Web.Pages.Akce;

public sealed class ContactModel(
    IContactService contactService,
    IRateLimitService rateLimitService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Vyplň jméno.")]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Vyplň e-mail.")]
    [EmailAddress(ErrorMessage = "Vyplň platný e-mail.")]
    [StringLength(180)]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Vyplň zprávu.")]
    [StringLength(2500)]
    public string Message { get; set; } = string.Empty;

    [BindProperty]
    public string SourcePage { get; set; } = string.Empty;

    [BindProperty]
    public string ReturnUrl { get; set; } = "/";

    [BindProperty]
    public string Honeypot { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var safeReturn = Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : Url.Page("/Index")!;

        if (!string.IsNullOrWhiteSpace(Honeypot))
        {
            TempData["FlashMessage"] = "Zprávu se nepodařilo odeslat.";
            TempData["FlashType"] = "error";
            return LocalRedirect(safeReturn);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!rateLimitService.IsAllowed($"contact:{ip}", 5, TimeSpan.FromMinutes(10)))
        {
            TempData["FlashMessage"] = "Posíláš příliš mnoho požadavků. Zkus to prosím za chvíli.";
            TempData["FlashType"] = "error";
            return LocalRedirect(safeReturn);
        }

        if (!ModelState.IsValid)
        {
            TempData["FlashMessage"] = "Prosím zkontroluj vyplněná data ve formuláři.";
            TempData["FlashType"] = "error";
            return LocalRedirect(safeReturn);
        }

        var result = await contactService.SubmitAsync(new ContactMessageInput
        {
            FullName = FullName,
            Email = Email,
            Message = Message,
            SourcePage = SourcePage,
            Honeypot = Honeypot
        }, ip, cancellationToken);

        TempData["FlashMessage"] = result.Message;
        TempData["FlashType"] = result.Succeeded ? "success" : "error";
        return LocalRedirect(safeReturn);
    }
}

