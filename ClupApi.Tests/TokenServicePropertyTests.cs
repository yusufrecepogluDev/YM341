using ClupApi.Services;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ClupApi;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace ClupApi.Tests
{
    /// <summary>
    /// Property-based tests for TokenService
    /// **Feature: api-security**
    /// </summary>
    public class TokenServicePropertyTests
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public TokenServicePropertyTests()
        {
            var configValues = new Dictionary<string, string?>
            {
                {"JwtSettings:SecretKey", "ThisIsAVeryLongSecretKeyForTestingPurposes123!"},
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"}
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _tokenService = new TokenService(_config, _context);
        }

        /// <summary>
        /// **Property 10: JWT token expiration**
        /// *For any* generated JWT access token, the expiration claim SHALL be set to approximately 30 minutes from generation time.
        /// **Validates: Requirements 5.1**
        /// </summary>
        [Property(MaxTest = 100)]
        public bool TokenExpiration_ShouldBe30Minutes(PositiveInt userId)
        {
            var beforeGeneration = DateTime.UtcNow;
            var token = _tokenService.GenerateAccessToken(
                userId.Get, 
                "student", 
                userId.Get.ToString());
            var afterGeneration = DateTime.UtcNow;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var expiration = jwtToken.ValidTo;

            // Token should expire approximately 30 minutes after generation (with 5 second tolerance)
            var expectedMinExpiration = beforeGeneration.AddMinutes(30).AddSeconds(-5);
            var expectedMaxExpiration = afterGeneration.AddMinutes(30).AddSeconds(5);

            return expiration >= expectedMinExpiration && expiration <= expectedMaxExpiration;
        }


        /// <summary>
        /// **Property 11: JWT token claims completeness**
        /// *For any* generated JWT token, the token SHALL contain user ID (sub), userType, and issued-at (iat) claims.
        /// **Validates: Requirements 5.2**
        /// </summary>
        [Theory]
        [InlineData(1, "student", "12345678")]
        [InlineData(2, "club", "87654321")]
        [InlineData(100, "student", "11111111")]
        [InlineData(999, "club", "99999999")]
        public void TokenClaims_ShouldContainRequiredClaims(int userId, string userType, string identifier)
        {
            var token = _tokenService.GenerateAccessToken(userId, userType, identifier);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var hasSub = jwtToken.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Sub);
            var hasUserType = jwtToken.Claims.Any(c => c.Type == "userType");
            var hasIat = jwtToken.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Iat);
            var hasUserId = jwtToken.Claims.Any(c => c.Type == "userId");

            // Verify userType claim value matches input
            var userTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userType");
            var userTypeMatches = userTypeClaim?.Value == userType;

            Assert.True(hasSub, "Token should contain 'sub' claim");
            Assert.True(hasUserType, "Token should contain 'userType' claim");
            Assert.True(hasIat, "Token should contain 'iat' claim");
            Assert.True(hasUserId, "Token should contain 'userId' claim");
            Assert.True(userTypeMatches, "userType claim should match input");
        }

        /// <summary>
        /// Additional property: Refresh token should be unique
        /// </summary>
        [Property(MaxTest = 50)]
        public async Task<bool> RefreshToken_ShouldBeUnique(PositiveInt userId)
        {
            var token1 = await _tokenService.GenerateRefreshTokenAsync(userId.Get, "student");
            var token2 = await _tokenService.GenerateRefreshTokenAsync(userId.Get, "student");

            return token1.Token != token2.Token;
        }

        /// <summary>
        /// Additional property: Refresh token expiration should be 7 days
        /// </summary>
        [Property(MaxTest = 100)]
        public async Task<bool> RefreshTokenExpiration_ShouldBe7Days(PositiveInt userId)
        {
            var beforeGeneration = DateTime.UtcNow;
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(userId.Get, "student");
            var afterGeneration = DateTime.UtcNow;

            var expectedMinExpiration = beforeGeneration.AddDays(7);
            var expectedMaxExpiration = afterGeneration.AddDays(7).AddSeconds(1);

            return refreshToken.ExpiresAt >= expectedMinExpiration && 
                   refreshToken.ExpiresAt <= expectedMaxExpiration;
        }
    }
}
