using Vbodlaci.Web.Application.Common;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Application.Newsletter;

public interface INewsletterService
{
    Task<ServiceResult> SubscribeAsync(NewsletterSubscribeInput input, string clientIp, CancellationToken cancellationToken = default);

    Task<ServiceResult> UnsubscribeAsync(string token, CancellationToken cancellationToken = default);

    Task<int> NotifyCoursePublishedAsync(Course course, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NewsletterSubscriberItem>> GetSubscribersAsync(CancellationToken cancellationToken = default);

    Task<string> ExportSubscribersCsvAsync(CancellationToken cancellationToken = default);
}
