using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;

namespace KampusEtkinlik.Services
{
    public class TokenService
    {
        private readonly ProtectedSessionStorage _sessionStorage;

        public TokenService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task SaveTokenAsync(string token, string userType, int userId)
        {
            System.Diagnostics.Debug.WriteLine($"SaveTokenAsync - Saving token (length: {token?.Length ?? 0}), userType: '{userType}', userId: {userId}");
            
            await _sessionStorage.SetAsync("authToken", token);
            await _sessionStorage.SetAsync("userType", userType);
            await _sessionStorage.SetAsync("userId", userId);
            
            System.Diagnostics.Debug.WriteLine("SaveTokenAsync - Token saved successfully");
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<string>("authToken");
                var token = result.Success ? result.Value : null;
                System.Diagnostics.Debug.WriteLine($"GetTokenAsync - Token from sessionStorage: {(string.IsNullOrEmpty(token) ? "null/empty" : $"exists (length: {token.Length})")}");
                return token;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetUserTypeAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<string>("userType");
                var userType = result.Success ? result.Value : null;
                System.Diagnostics.Debug.WriteLine($"GetUserTypeAsync: {userType ?? "null"}");
                return userType;
            }
            catch
            {
                return null;
            }
        }

        public async Task<int?> GetUserIdAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<int>("userId");
                return result.Success ? result.Value : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task ClearTokenAsync()
        {
            await _sessionStorage.DeleteAsync("authToken");
            await _sessionStorage.DeleteAsync("userType");
            await _sessionStorage.DeleteAsync("userId");
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var now = DateTime.UtcNow;
                var isExpired = jwtToken.ValidTo < now;
                
                System.Diagnostics.Debug.WriteLine($"IsTokenExpired - ValidTo: {jwtToken.ValidTo}, Now: {now}, Expired: {isExpired}");
                
                return isExpired;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsTokenExpired - Error: {ex.Message}");
                return true;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            System.Diagnostics.Debug.WriteLine($"IsAuthenticatedAsync - Token: {(string.IsNullOrEmpty(token) ? "null/empty" : "exists")}");
            
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("IsAuthenticatedAsync - Token is null or empty, returning false");
                return false;
            }

            var isExpired = IsTokenExpired(token);
            System.Diagnostics.Debug.WriteLine($"IsAuthenticatedAsync - Token expired: {isExpired}");
            return !isExpired;
        }
    }
}
