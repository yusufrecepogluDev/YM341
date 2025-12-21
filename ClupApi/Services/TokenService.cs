using ClupApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ClupApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private const int AccessTokenExpirationMinutes = 30;
        private const int RefreshTokenExpirationDays = 7;

        public TokenService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public string GenerateAccessToken(int userId, string userType, string identifier)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, identifier.ToString()),
                new Claim("userId", userId.ToString()),
                new Claim("userType", userType),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<RefreshToken> GenerateRefreshTokenAsync(int userId, string userType)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                UserId = userId,
                UserType = userType,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
                IsRevoked = false
            };

            // Save to database
            try
            {
                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"RefreshToken kaydetme hatası: {innerMessage}", ex);
            }

            return refreshToken;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken == null)
                return false;

            if (refreshToken.IsRevoked)
                return false;

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
                return false;

            return true;
        }

        public async Task RevokeRefreshTokenAsync(string token, string? reason = null)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedReason = reason ?? "User logout";
                await _context.SaveChangesAsync();
            }
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);
        }

        public ClaimsPrincipal? ValidateAccessToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
