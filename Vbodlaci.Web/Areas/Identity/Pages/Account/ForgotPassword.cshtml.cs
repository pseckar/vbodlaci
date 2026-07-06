using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Vbodlaci.Web.Application.Security;
using Vbodlaci.Web.Services.Email;

namespace Vbodlaci.Web.Areas.Identity.Pages.Account;

public sealed class ForgotPasswordModel(
    UserManager<IdentityUser> userManager,
    IEmailDispatcher emailDispatcher,
    IRateLimitService rateLimitService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Vyplň e-mail.")]
        [EmailAddress(ErrorMessage = "Vyplň platný e-mail.")]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!rateLimitService.IsAllowed($"password-reset:{ip}", 5, TimeSpan.FromMinutes(15)))
        {
            ModelState.AddModelError(string.Empty, "Posíláš příliš mnoho požadavků. Zkus to prosím za chvíli.");
            return Page();
        }

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user is not null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme)!;

            var textBody =
                "Dobrý den,\n\n" +
                "pro účet na webu V bodláčí byla vyžádána obnova hesla. Nové heslo nastavíte na tomto odkazu:\n\n" +
                $"{callbackUrl}\n\n" +
                "Pokud jste o obnovu hesla nežádali, tento e-mail můžete ignorovat.\n\n" +
                "V bodláčí";

            await emailDispatcher.SendAsync(
                kind: "password-reset",
                to: Input.Email,
                subject: "Obnova hesla — V bodláčí",
                textBody: textBody,
                cancellationToken: cancellationToken);
        }

        // do not reveal whether the account exists
        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}
