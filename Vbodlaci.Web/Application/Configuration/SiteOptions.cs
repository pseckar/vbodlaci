namespace Vbodlaci.Web.Application.Configuration;

public sealed class SiteOptions
{
    public const string SectionName = "Site";

    public string SiteUrl { get; set; } = "https://vbodlaci.cz";

    public string ContactInboxEmail { get; set; } = "kontakt@vbodlaci.cz";

    public string RegistrationInboxEmail { get; set; } = "kontakt@vbodlaci.cz";

    public string FacebookUrl { get; set; } = "#";

    public string InstagramUrl { get; set; } = "#";
}
