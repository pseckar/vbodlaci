using Vbodlaci.Web.Application.Common;

namespace Vbodlaci.Web.Application.Registrations;

public interface IRegistrationService
{
    Task<ServiceResult> RegisterAsync(Guid courseId, CourseRegistrationInput input, string clientIp, CancellationToken cancellationToken = default);
}
