namespace Vbodlaci.Web.Application.Presentation;

/// <summary>
/// Parameters for the shared contact + newsletter section (_ContactNewsletter.cshtml).
/// Form field names and posted values must stay aligned with
/// Pages/Actions/Contact and Pages/Actions/Newsletter handlers.
/// </summary>
public sealed class ContactNewsletterViewModel
{
    /// <summary>Posted as SourcePage with the contact form ("home", "breathwork", "kone", "veterina").</summary>
    public required string SourcePage { get; init; }

    /// <summary>Short unique prefix for honeypot input ids on this page (e.g. "home", "bw").</summary>
    public required string HoneypotIdPrefix { get; init; }

    public required string ReturnUrl { get; init; }

    public string SectionId { get; init; } = "kontakt";

    public string ContactKicker { get; init; } = "napiš mi";

    public string ContactHeading { get; init; } = "Kontakt";

    public string? ContactIntro { get; init; }

    public string MessagePlaceholder { get; init; } = "Zpráva";

    public string ContactSubmitLabel { get; init; } = "Odeslat zprávu";

    /// <summary>Optional handwritten note under the contact form.</summary>
    public string? ContactNote { get; init; }

    public string NewsletterKicker { get; init; } = "zůstaňme v kontaktu";

    public string? NewsletterIntro { get; init; }

    public bool PrefBreathworkChecked { get; init; }

    public bool PrefHorsesChecked { get; init; }

    public bool PrefVeterinaryChecked { get; init; }

    public bool ShowThistleWatermark { get; init; } = true;
}
