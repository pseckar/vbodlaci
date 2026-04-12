using System.ComponentModel.DataAnnotations;

namespace Vbodlaci.Web.Application.Registrations;

public sealed class CourseRegistrationInput
{
    [Required(ErrorMessage = "Vyplň jméno.")]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vyplň e-mail.")]
    [EmailAddress(ErrorMessage = "Vyplň platný e-mail.")]
    [StringLength(180)]
    public string Email { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Note { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Je potřeba souhlasit s podmínkami kurzu.")]
    public bool TermsConsent { get; set; }

    public string Honeypot { get; set; } = string.Empty;
}
