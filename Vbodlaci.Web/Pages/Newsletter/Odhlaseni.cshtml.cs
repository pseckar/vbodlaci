using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Newsletter;

namespace Vbodlaci.Web.Pages.Newsletter;

public sealed class UnsubscribeModel(INewsletterService newsletterService) : PageModel
{
    public string Message { get; private set; } = string.Empty;

    public async Task OnGetAsync(string token, CancellationToken cancellationToken)
    {
        var result = await newsletterService.UnsubscribeAsync(token, cancellationToken);
        Message = result.Message;
    }
}

