namespace ClupApi.Middleware;


public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers directly to response
        AddSecurityHeaders(context);

        await _next(context);
    }


    public static void AddSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent MIME type sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // Prevent clickjacking
        headers["X-Frame-Options"] = "DENY";

        // Enable XSS filter in browsers
        headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer policy
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Content Security Policy (basic)
        headers["Content-Security-Policy"] = "default-src 'self'";

        // Permissions Policy (disable unnecessary features)
        headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        // HSTS - only in production with HTTPS
        if (!context.Request.Host.Host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }
    }
}


public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
