namespace Vbodlaci.Web.Application.Security;

public interface IRateLimitService
{
    bool IsAllowed(string key, int maxAttempts, TimeSpan window);
}
