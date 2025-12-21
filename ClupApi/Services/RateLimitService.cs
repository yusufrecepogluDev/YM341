using System.Collections.Concurrent;

namespace ClupApi.Services;


public class RateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();
    private readonly ILogger<RateLimitService> _logger;

    // Rate limit configurations
    private static readonly Dictionary<string, RateLimitConfig> _configs = new()
    {
        { "login", new RateLimitConfig(5, TimeSpan.FromMinutes(15)) },
        { "register", new RateLimitConfig(10, TimeSpan.FromHours(1)) }
    };

    public RateLimitService(ILogger<RateLimitService> logger)
    {
        _logger = logger;
    }

    public Task<bool> IsRateLimitedAsync(string clientIp, string endpoint)
    {
        var key = GetKey(clientIp, endpoint);
        var config = GetConfig(endpoint);

        if (!_entries.TryGetValue(key, out var entry))
        {
            return Task.FromResult(false);
        }

        // Check if window has expired
        if (DateTime.UtcNow > entry.WindowStart.Add(config.Window))
        {
            // Window expired, reset
            _entries.TryRemove(key, out _);
            return Task.FromResult(false);
        }

        // Check if blocked
        if (entry.BlockedUntil.HasValue && DateTime.UtcNow < entry.BlockedUntil.Value)
        {
            return Task.FromResult(true);
        }

        // Check if limit exceeded
        if (entry.RequestCount >= config.MaxRequests)
        {
            // Block until window expires
            entry.BlockedUntil = entry.WindowStart.Add(config.Window);
            _logger.LogWarning("Rate limit exceeded for {ClientIp} on {Endpoint}", clientIp, endpoint);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task RecordRequestAsync(string clientIp, string endpoint)
    {
        var key = GetKey(clientIp, endpoint);
        var config = GetConfig(endpoint);
        var now = DateTime.UtcNow;

        _entries.AddOrUpdate(key,
            // Add new entry
            _ => new RateLimitEntry
            {
                ClientIp = clientIp,
                Endpoint = endpoint,
                RequestCount = 1,
                WindowStart = now,
                BlockedUntil = null
            },
            // Update existing entry
            (_, existing) =>
            {
                // Check if window has expired
                if (now > existing.WindowStart.Add(config.Window))
                {
                    // Reset window
                    return new RateLimitEntry
                    {
                        ClientIp = clientIp,
                        Endpoint = endpoint,
                        RequestCount = 1,
                        WindowStart = now,
                        BlockedUntil = null
                    };
                }

                // Increment count
                existing.RequestCount++;
                return existing;
            });

        return Task.CompletedTask;
    }

    public TimeSpan? GetRetryAfter(string clientIp, string endpoint)
    {
        var key = GetKey(clientIp, endpoint);
        var config = GetConfig(endpoint);

        if (!_entries.TryGetValue(key, out var entry))
        {
            return null;
        }

        if (entry.BlockedUntil.HasValue && DateTime.UtcNow < entry.BlockedUntil.Value)
        {
            return entry.BlockedUntil.Value - DateTime.UtcNow;
        }

        // If at limit but not blocked yet, calculate when window expires
        if (entry.RequestCount >= config.MaxRequests)
        {
            var windowEnd = entry.WindowStart.Add(config.Window);
            if (DateTime.UtcNow < windowEnd)
            {
                return windowEnd - DateTime.UtcNow;
            }
        }

        return null;
    }

    public void ClearRateLimit(string clientIp, string endpoint)
    {
        var key = GetKey(clientIp, endpoint);
        _entries.TryRemove(key, out _);
    }

    private static string GetKey(string clientIp, string endpoint)
    {
        var normalizedEndpoint = NormalizeEndpoint(endpoint);
        return $"{clientIp}:{normalizedEndpoint}";
    }

    private static string NormalizeEndpoint(string endpoint)
    {
        // Normalize endpoint to category (login or register)
        if (endpoint.Contains("login", StringComparison.OrdinalIgnoreCase))
            return "login";
        if (endpoint.Contains("register", StringComparison.OrdinalIgnoreCase))
            return "register";
        return endpoint.ToLowerInvariant();
    }

    private static RateLimitConfig GetConfig(string endpoint)
    {
        var normalized = NormalizeEndpoint(endpoint);
        return _configs.TryGetValue(normalized, out var config)
            ? config
            : new RateLimitConfig(100, TimeSpan.FromMinutes(1)); // Default fallback
    }
}

public record RateLimitConfig(int MaxRequests, TimeSpan Window);

public class RateLimitEntry
{
    public required string ClientIp { get; set; }
    public required string Endpoint { get; set; }
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime? BlockedUntil { get; set; }
}
