using ClupApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClupApi.Tests;

/// <summary>
/// Property-based tests for RateLimitService
/// **Feature: api-security, Property 5: Rate limiting blocks excessive login attempts**
/// **Validates: Requirements 3.1, 3.2, 3.3**
/// </summary>
public class RateLimitServicePropertyTests
{
    private readonly RateLimitService _rateLimitService;

    public RateLimitServicePropertyTests()
    {
        var loggerMock = new Mock<ILogger<RateLimitService>>();
        _rateLimitService = new RateLimitService(loggerMock.Object);
    }

    /// <summary>
    /// Property: After 5 login attempts, subsequent requests should be rate limited
    /// </summary>
    [Fact]
    public async Task LoginRateLimit_After5Attempts_ShouldBeBlocked()
    {
        // Arrange
        var clientIp = "192.168.1.100";
        var endpoint = "/api/auth/student/login";

        // Clear any existing rate limit
        _rateLimitService.ClearRateLimit(clientIp, endpoint);

        // Act - Make 5 requests (the limit)
        for (int i = 0; i < 5; i++)
        {
            var isLimited = await _rateLimitService.IsRateLimitedAsync(clientIp, endpoint);
            Assert.False(isLimited, $"Request {i + 1} should not be rate limited");
            await _rateLimitService.RecordRequestAsync(clientIp, endpoint);
        }

        // Assert - 6th request should be blocked
        var isBlocked = await _rateLimitService.IsRateLimitedAsync(clientIp, endpoint);
        Assert.True(isBlocked, "6th request should be rate limited");
    }

    /// <summary>
    /// Property: After 10 register attempts, subsequent requests should be rate limited
    /// </summary>
    [Fact]
    public async Task RegisterRateLimit_After10Attempts_ShouldBeBlocked()
    {
        // Arrange
        var clientIp = "192.168.1.101";
        var endpoint = "/api/auth/student/register";

        // Clear any existing rate limit
        _rateLimitService.ClearRateLimit(clientIp, endpoint);

        // Act - Make 10 requests (the limit)
        for (int i = 0; i < 10; i++)
        {
            var isLimited = await _rateLimitService.IsRateLimitedAsync(clientIp, endpoint);
            Assert.False(isLimited, $"Request {i + 1} should not be rate limited");
            await _rateLimitService.RecordRequestAsync(clientIp, endpoint);
        }

        // Assert - 11th request should be blocked
        var isBlocked = await _rateLimitService.IsRateLimitedAsync(clientIp, endpoint);
        Assert.True(isBlocked, "11th request should be rate limited");
    }

    /// <summary>
    /// Property: Rate limit should return Retry-After value when blocked
    /// </summary>
    [Fact]
    public async Task RateLimit_WhenBlocked_ShouldReturnRetryAfter()
    {
        // Arrange
        var clientIp = "192.168.1.102";
        var endpoint = "/api/auth/club/login";

        _rateLimitService.ClearRateLimit(clientIp, endpoint);

        // Act - Exceed the limit
        for (int i = 0; i < 6; i++)
        {
            await _rateLimitService.RecordRequestAsync(clientIp, endpoint);
        }

        // Trigger the block check
        await _rateLimitService.IsRateLimitedAsync(clientIp, endpoint);

        // Assert
        var retryAfter = _rateLimitService.GetRetryAfter(clientIp, endpoint);
        Assert.NotNull(retryAfter);
        Assert.True(retryAfter.Value.TotalSeconds > 0, "Retry-After should be positive");
        Assert.True(retryAfter.Value.TotalMinutes <= 15, "Retry-After should not exceed 15 minutes for login");
    }

    /// <summary>
    /// Property: Different IPs should have independent rate limits
    /// </summary>
    [Theory]
    [InlineData(1, 2)]
    [InlineData(100, 200)]
    [InlineData(50, 150)]
    public async Task DifferentIPs_ShouldHaveIndependentRateLimits(int ip1Suffix, int ip2Suffix)
    {
        var clientIp1 = $"10.0.0.{ip1Suffix % 256}";
        var clientIp2 = $"10.0.1.{ip2Suffix % 256}";
        var endpoint = "/api/auth/student/login";

        // Clear rate limits
        _rateLimitService.ClearRateLimit(clientIp1, endpoint);
        _rateLimitService.ClearRateLimit(clientIp2, endpoint);

        // Record requests for IP1 only
        for (int i = 0; i < 5; i++)
        {
            await _rateLimitService.RecordRequestAsync(clientIp1, endpoint);
        }

        // IP1 should be at limit, IP2 should not be affected
        var ip1Limited = await _rateLimitService.IsRateLimitedAsync(clientIp1, endpoint);
        var ip2Limited = await _rateLimitService.IsRateLimitedAsync(clientIp2, endpoint);

        // IP2 should not be limited
        Assert.False(ip2Limited, "IP2 should not be affected by IP1's rate limit");
    }

    /// <summary>
    /// Property: Login and register endpoints should have different limits
    /// </summary>
    [Fact]
    public async Task DifferentEndpoints_ShouldHaveDifferentLimits()
    {
        // Arrange
        var clientIp = "192.168.1.103";
        var loginEndpoint = "/api/auth/student/login";
        var registerEndpoint = "/api/auth/student/register";

        _rateLimitService.ClearRateLimit(clientIp, loginEndpoint);
        _rateLimitService.ClearRateLimit(clientIp, registerEndpoint);

        // Act - Make 5 login requests (at limit)
        for (int i = 0; i < 5; i++)
        {
            await _rateLimitService.RecordRequestAsync(clientIp, loginEndpoint);
        }

        // Assert - Login should be at limit, register should not
        var loginLimited = await _rateLimitService.IsRateLimitedAsync(clientIp, loginEndpoint);
        var registerLimited = await _rateLimitService.IsRateLimitedAsync(clientIp, registerEndpoint);

        Assert.True(loginLimited, "Login should be rate limited after 5 attempts");
        Assert.False(registerLimited, "Register should not be affected by login rate limit");
    }

    /// <summary>
    /// Property: New client should never be rate limited on first request
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(255)]
    [InlineData(500)]
    [InlineData(1000)]
    public async Task NewClient_ShouldNeverBeRateLimited(int ipSuffix)
    {
        var clientIp = $"172.16.{(ipSuffix / 256) % 256}.{ipSuffix % 256}";
        var endpoint = "/api/auth/student/login";

        // Clear any existing rate limit
        _rateLimitService.ClearRateLimit(clientIp, endpoint);

        // First request should never be limited
        var isLimited = await _rateLimitService.IsRateLimitedAsync(clientIp, endpoint);
        Assert.False(isLimited, "New client should never be rate limited on first request");
    }

    /// <summary>
    /// Property: Club login endpoints should have same rate limit as student login
    /// </summary>
    [Fact]
    public async Task ClubLogin_ShouldHaveSameRateLimitAsStudentLogin()
    {
        // Arrange
        var clientIp1 = "192.168.1.104";
        var clientIp2 = "192.168.1.105";
        var studentLogin = "/api/auth/student/login";
        var clubLogin = "/api/auth/club/login";

        _rateLimitService.ClearRateLimit(clientIp1, studentLogin);
        _rateLimitService.ClearRateLimit(clientIp2, clubLogin);

        // Act - Make 5 requests on each
        for (int i = 0; i < 5; i++)
        {
            await _rateLimitService.RecordRequestAsync(clientIp1, studentLogin);
            await _rateLimitService.RecordRequestAsync(clientIp2, clubLogin);
        }

        // Assert - Both should be at limit
        var studentLimited = await _rateLimitService.IsRateLimitedAsync(clientIp1, studentLogin);
        var clubLimited = await _rateLimitService.IsRateLimitedAsync(clientIp2, clubLogin);

        Assert.True(studentLimited, "Student login should be rate limited");
        Assert.True(clubLimited, "Club login should be rate limited");
    }
}
