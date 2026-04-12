using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Contacts;

namespace Vbodlaci.Web.Areas.Admin.Pages.Contacts;

public sealed class DetailModel(IContactService contactService) : PageModel
{
    public ContactInboxItem? Message { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Message = await contactService.GetByIdAsync(id, cancellationToken);
        return Message is null ? NotFound() : Page();
    }
}
