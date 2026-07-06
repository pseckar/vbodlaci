using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Vbodlaci.Web.Areas.Identity.Pages.Account;

public sealed class LoginModel(SignInManager<IdentityUser> signInManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Vyplň e-mail.")]
        [EmailAddress(ErrorMessage = "Vyplň platný e-mail.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vyplň heslo.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        var safeReturn = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/Admin");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return LocalRedirect(safeReturn);
        }

        ModelState.AddModelError(string.Empty, "Nesprávný e-mail nebo heslo.");
        return Page();
    }
}
