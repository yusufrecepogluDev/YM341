using ClupApi.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ClupApi.Tests;

/// <summary>
/// Property-based tests for SecurityHeadersMiddleware
/// **Feature: api-security, Property 12: Security headers presence**
/// **Validates: Requirements 6.1, 6.2, 6.3, 6.4**
/// </summary>
public class SecurityHeadersPropertyTests
{
    /// <summary>
    /// Property: All responses should include X-Content-Type-Options header
    /// </summary>
    [Fact]
    public async Task Response_ShouldInclude_XContentTypeOptions()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Content-Type-Options"));
        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
    }

    /// <summary>
    /// Property: All responses should include X-Frame-Options header
    /// </summary>
    [Fact]
    public async Task Response_ShouldInclude_XFrameOptions()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Frame-Options"));
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);
    }

    /// <summary>
    /// Property: All responses should include X-XSS-Protection header
    /// </summary>
    [Fact]
    public async Task Response_ShouldInclude_XXSSProtection()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-XSS-Protection"));
        Assert.Equal("1; mode=block", context.Response.Headers["X-XSS-Protection"]);
    }

    /// <summary>
    /// Property: All responses should include Referrer-Policy header
    /// </summary>
    [Fact]
    public async Task Response_ShouldInclude_ReferrerPolicy()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Referrer-Policy"));
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"]);
    }

    /// <summary>
    /// Property: All responses should include Content-Security-Policy header
    /// </summary>
    [Fact]
    public async Task Response_ShouldInclude_ContentSecurityPolicy()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Content-Security-Policy"));
        Assert.Equal("default-src 'self'", context.Response.Headers["Content-Security-Policy"]);
    }

    /// <summary>
    /// Property: Localhost requests should NOT include HSTS header
    /// </summary>
    [Fact]
    public async Task LocalhostResponse_ShouldNotInclude_HSTS()
    {
        // Arrange
        var context = CreateHttpContext("localhost");
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
    }

    /// <summary>
    /// Property: Production requests should include HSTS header
    /// </summary>
    [Fact]
    public async Task ProductionResponse_ShouldInclude_HSTS()
    {
        // Arrange
        var context = CreateHttpContext("api.example.com");
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
        Assert.Contains("max-age=", context.Response.Headers["Strict-Transport-Security"].ToString());
    }

    /// <summary>
    /// Property: Middleware should call next delegate
    /// </summary>
    [Fact]
    public async Task Middleware_ShouldCallNextDelegate()
    {
        // Arrange
        var context = CreateHttpContext();
        var nextCalled = false;
        var middleware = new SecurityHeadersMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    /// <summary>
    /// Property: All required security headers should be present in a single response
    /// </summary>
    [Fact]
    public async Task Response_ShouldIncludeAllRequiredSecurityHeaders()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert - All required headers present
        var requiredHeaders = new[]
        {
            "X-Content-Type-Options",
            "X-Frame-Options",
            "X-XSS-Protection",
            "Referrer-Policy",
            "Content-Security-Policy",
            "Permissions-Policy"
        };

        foreach (var header in requiredHeaders)
        {
            Assert.True(context.Response.Headers.ContainsKey(header), $"Missing header: {header}");
        }
    }

    private static DefaultHttpContext CreateHttpContext(string host = "localhost")
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString(host);
        return context;
    }
}
