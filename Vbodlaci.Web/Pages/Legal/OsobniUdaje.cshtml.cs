using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Vbodlaci.Web.Application.Configuration;

namespace Vbodlaci.Web.Pages.Legal;

public sealed class PersonalDataModel(IOptions<LegalIdentityOptions> legalOptions) : PageModel
{
    public string BusinessName => legalOptions.Value.BusinessName;

    public string Address => legalOptions.Value.Address;

    public string CompanyId => legalOptions.Value.CompanyId;

    public string ContactEmail => legalOptions.Value.ContactEmail;

    public string ContactPhone => legalOptions.Value.ContactPhone;

    public bool ContainsPlaceholder =>
        BusinessName.Contains("TODO", StringComparison.OrdinalIgnoreCase)
        || Address.Contains("TODO", StringComparison.OrdinalIgnoreCase)
        || CompanyId.Contains("TODO", StringComparison.OrdinalIgnoreCase)
        || ContactEmail.Contains("TODO", StringComparison.OrdinalIgnoreCase)
        || ContactPhone.Contains("TODO", StringComparison.OrdinalIgnoreCase);
}

