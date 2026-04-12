using System.ComponentModel.DataAnnotations;

namespace Vbodlaci.Web.Application.Contacts;

public sealed class ContactMessageInput
{
    [Required(ErrorMessage = "Vyplň jméno.")]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vyplň e-mail.")]
    [EmailAddress(ErrorMessage = "Vyplň platný e-mail.")]
    [StringLength(180)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vyplň zprávu.")]
    [StringLength(2500)]
    public string Message { get; set; } = string.Empty;

    public string SourcePage { get; set; } = string.Empty;

    public string Honeypot { get; set; } = string.Empty;
}
