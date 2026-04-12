using System.ComponentModel.DataAnnotations;

namespace Vbodlaci.Web.Application.Newsletter;

public sealed class NewsletterSubscribeInput
{
    [Required(ErrorMessage = "Vyplň e-mail.")]
    [EmailAddress(ErrorMessage = "Vyplň platný e-mail.")]
    [StringLength(180)]
    public string Email { get; set; } = string.Empty;

    public bool PrefBreathwork { get; set; }

    public bool PrefKone { get; set; }//TODO: as mentioned, English only, no Czech (with exception in strings for user-facing texts). Change for also other names, not just this

    public bool PrefVeterina { get; set; }

    public string Honeypot { get; set; } = string.Empty;
}
