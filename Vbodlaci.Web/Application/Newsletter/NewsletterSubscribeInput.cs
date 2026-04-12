using System.ComponentModel.DataAnnotations;

namespace Vbodlaci.Web.Application.Newsletter;

public sealed class NewsletterSubscribeInput
{
    [Required(ErrorMessage = "Vyplň e-mail.")]
    [EmailAddress(ErrorMessage = "Vyplň platný e-mail.")]
    [StringLength(180)]
    public string Email { get; set; } = string.Empty;

    public bool PrefBreathwork { get; set; }

    public bool PrefHorses { get; set; }

    public bool PrefVeterinary { get; set; }

    public string Honeypot { get; set; } = string.Empty;
}


