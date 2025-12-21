using ClupApi.Models;
using System.Security.Claims;

namespace ClupApi.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(int userId, string userType, string identifier);
        Task<RefreshToken> GenerateRefreshTokenAsync(int userId, string userType);
        Task<bool> ValidateRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string? reason = null);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        ClaimsPrincipal? ValidateAccessToken(string token);
    }
}
