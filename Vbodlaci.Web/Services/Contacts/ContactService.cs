using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vbodlaci.Web.Application.Common;
using Vbodlaci.Web.Application.Configuration;
using Vbodlaci.Web.Application.Contacts;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Contacts;
using Vbodlaci.Web.Services.Email;

namespace Vbodlaci.Web.Services.Contacts;

public sealed class ContactService(
    ApplicationDbContext dbContext,
    IEmailDispatcher emailDispatcher,
    IOptions<SiteOptions> siteOptions) : IContactService
{
    public async Task<ServiceResult> SubmitAsync(ContactMessageInput input, string clientIp, CancellationToken cancellationToken = default)
    {
        var inquiry = new ContactInquiry
        {
            FullName = input.FullName.Trim(),
            Email = input.Email.Trim(),
            Message = input.Message.Trim(),
            SourcePage = string.IsNullOrWhiteSpace(input.SourcePage) ? "unknown" : input.SourcePage.Trim(),
            ClientIp = clientIp,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.ContactInquiries.Add(inquiry);
        await dbContext.SaveChangesAsync(cancellationToken);

        var subject = $"Nový dotaz z webu V bodláčí ({inquiry.SourcePage})";
        var body = $"Jméno: {inquiry.FullName}\nE-mail: {inquiry.Email}\nZdroj: {inquiry.SourcePage}\n\nZpráva:\n{inquiry.Message}";

        await emailDispatcher.SendAsync("ContactInquiry", siteOptions.Value.ContactInboxEmail, subject, body, cancellationToken: cancellationToken);

        return ServiceResult.Success("Děkujeme, zpráva byla odeslána.");
    }

    public async Task<IReadOnlyList<ContactInboxItem>> GetInboxAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ContactInquiries
            .AsNoTracking()
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new ContactInboxItem
            {
                Id = item.Id,
                FullName = item.FullName,
                Email = item.Email,
                Message = item.Message,
                SourcePage = item.SourcePage,
                CreatedAt = item.CreatedAt,
                ClientIp = item.ClientIp
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ContactInboxItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ContactInquiries
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new ContactInboxItem
            {
                Id = item.Id,
                FullName = item.FullName,
                Email = item.Email,
                Message = item.Message,
                SourcePage = item.SourcePage,
                CreatedAt = item.CreatedAt,
                ClientIp = item.ClientIp
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
