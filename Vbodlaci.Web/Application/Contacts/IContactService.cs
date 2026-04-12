using Vbodlaci.Web.Application.Common;

namespace Vbodlaci.Web.Application.Contacts;

public interface IContactService
{
    Task<ServiceResult> SubmitAsync(ContactMessageInput input, string clientIp, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContactInboxItem>> GetInboxAsync(CancellationToken cancellationToken = default);

    Task<ContactInboxItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
