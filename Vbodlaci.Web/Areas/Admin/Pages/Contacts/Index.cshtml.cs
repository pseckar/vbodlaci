using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Contacts;

namespace Vbodlaci.Web.Areas.Admin.Pages.Contacts;

public sealed class IndexModel(IContactService contactService) : PageModel
{
    public IReadOnlyList<ContactInboxItem> Messages { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Messages = await contactService.GetInboxAsync(cancellationToken);
    }
}
