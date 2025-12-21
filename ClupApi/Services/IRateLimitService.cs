namespace ClupApi.Services;


public interface IRateLimitService
{
    Task<bool> IsRateLimitedAsync(string clientIp, string endpoint);

    Task RecordRequestAsync(string clientIp, string endpoint);

    TimeSpan? GetRetryAfter(string clientIp, string endpoint);

    void ClearRateLimit(string clientIp, string endpoint);
}
