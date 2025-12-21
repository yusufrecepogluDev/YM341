using ClupApi.Models;
using ClupApi.Services;
using System.Text.Json;

namespace ClupApi.Middleware;


public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMiddleware> _logger;

    // Endpoints to rate limit
    private static readonly string[] RateLimitedEndpoints =
    {
        "/api/auth/student/login",
        "/api/auth/club/login",
        "/api/auth/student/register",
        "/api/auth/club/register"
    };

    public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Check if this endpoint should be rate limited
        if (!ShouldRateLimit(path))
        {
            await _next(context);
            return;
        }

        var clientIp = GetClientIp(context);

        // Check if client is rate limited
        if (await rateLimitService.IsRateLimitedAsync(clientIp, path))
        {
            var retryAfter = rateLimitService.GetRetryAfter(clientIp, path);
            
            _logger.LogWarning("Rate limit exceeded for {ClientIp} on {Path}", clientIp, path);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            if (retryAfter.HasValue)
            {
                context.Response.Headers["Retry-After"] = ((int)retryAfter.Value.TotalSeconds).ToString();
            }

            var response = ApiResponse.ErrorResponse("Too many requests. Please try again later.", Array.Empty<string>());
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
            return;
        }

        // Record this request
        await rateLimitService.RecordRequestAsync(clientIp, path);

        await _next(context);
    }

    private static bool ShouldRateLimit(string path)
    {
        return RateLimitedEndpoints.Any(endpoint => 
            path.Equals(endpoint, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetClientIp(HttpContext context)
    {
        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the chain (original client)
            return forwardedFor.Split(',')[0].Trim();
        }

        // Fall back to direct connection IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

public static class RateLimitMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitMiddleware>();
    }
}
