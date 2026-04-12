using Microsoft.Extensions.Caching.Memory;
using Vbodlaci.Web.Application.Security;

namespace Vbodlaci.Web.Services.Security;

public sealed class InMemoryRateLimitService(IMemoryCache cache) : IRateLimitService
{
    private sealed class Counter
    {
        public int Value { get; set; }
    }

    public bool IsAllowed(string key, int maxAttempts, TimeSpan window)
    {
        var counter = cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = window; //TODO: how does this work? When and where is the window checked for expiration?
            return new Counter();
        });

        if (counter is null)
        {
            return false;
        }

        counter.Value += 1;
        return counter.Value <= maxAttempts;
    }
}
