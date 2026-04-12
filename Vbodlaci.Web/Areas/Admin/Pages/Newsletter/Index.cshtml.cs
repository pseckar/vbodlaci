using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vbodlaci.Web.Application.Newsletter;

namespace Vbodlaci.Web.Areas.Admin.Pages.Newsletter;

public sealed class IndexModel(INewsletterService newsletterService) : PageModel
{
    public IReadOnlyList<NewsletterSubscriberItem> Subscribers { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Subscribers = await newsletterService.GetSubscribersAsync(cancellationToken);
    }

    public async Task<IActionResult> OnGetExportAsync(CancellationToken cancellationToken)
    {
        var csv = await newsletterService.ExportSubscribersCsvAsync(cancellationToken);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"newsletter-subscribers-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv");
    }
}
