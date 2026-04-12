namespace Vbodlaci.Web.Application.Configuration;

public sealed class LegalIdentityOptions
{
    public const string SectionName = "LegalIdentity";

    public string BusinessName { get; set; } = "TODO_DOPLNIT_OBCHODNI_JMENO";

    public string Address { get; set; } = "TODO_DOPLNIT_ADRESU";

    public string CompanyId { get; set; } = "TODO_DOPLNIT_ICO";

    public string ContactEmail { get; set; } = "TODO_DOPLNIT_EMAIL";

    public string ContactPhone { get; set; } = "TODO_DOPLNIT_TELEFON";
}
