using KampusEtkinlik.DTOs;
using KampusEtkinlik.Data.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace KampusEtkinlik.Services
{
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;
        private readonly MembershipCacheService _membershipCacheService;

        public AuthenticationService(HttpClient httpClient, IConfiguration configuration, TokenService tokenService, MembershipCacheService membershipCacheService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _tokenService = tokenService;
            _membershipCacheService = membershipCacheService;
        }

        public async Task SetAuthorizationHeaderAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<(bool Success, StudentLoginResponseDto? Data, string Message)> StudentLoginAsync(StudentLoginRequestDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/student/login", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<StudentLoginResponseDto>>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (true, result?.Data, result?.Message ?? "Giriş başarılı");
                }
                else
                {
                    // Try to parse as JSON error response
                    try
                    {
                        var errorResult = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (false, null, errorResult?.Message ?? "Giriş başarısız");
                    }
                    catch
                    {
                        // If not JSON, return the raw content or status code
                        return (false, null, $"Sunucu hatası ({(int)response.StatusCode}): {(content.Length > 100 ? content.Substring(0, 100) : content)}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, null, $"API'ye bağlanılamadı. Lütfen API'nin çalıştığından emin olun. Hata: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Bağlantı hatası: {ex.Message}");
            }
        }

        public async Task<(bool Success, ClubLoginResponseDto? Data, string Message)> ClubLoginAsync(ClubLoginRequestDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/club/login", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<ClubLoginResponseDto>>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (true, result?.Data, result?.Message ?? "Giriş başarılı");
                }
                else
                {
                    // Try to parse as JSON error response
                    try
                    {
                        var errorResult = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (false, null, errorResult?.Message ?? "Giriş başarısız");
                    }
                    catch
                    {
                        // If not JSON, return the raw content or status code
                        return (false, null, $"Sunucu hatası ({(int)response.StatusCode}): {(content.Length > 100 ? content.Substring(0, 100) : content)}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, null, $"API'ye bağlanılamadı. Lütfen API'nin çalıştığından emin olun. Hata: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Bağlantı hatası: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> StudentRegisterAsync(StudentRegisterRequestDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/student/register", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (true, result?.Message ?? "Kayıt başarılı");
                }
                else
                {
                    try
                    {
                        var errorResult = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (false, errorResult?.Message ?? "Kayıt başarısız");
                    }
                    catch
                    {
                        return (false, $"Sunucu hatası ({(int)response.StatusCode}): {(content.Length > 100 ? content.Substring(0, 100) : content)}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"API'ye bağlanılamadı. Lütfen API'nin çalıştığından emin olun. Hata: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ClubRegisterAsync(ClubRegisterRequestDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/club/register", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (true, result?.Message ?? "Kayıt başarılı");
                }
                else
                {
                    try
                    {
                        var errorResult = System.Text.Json.JsonSerializer.Deserialize<ApiResponse>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (false, errorResult?.Message ?? "Kayıt başarısız");
                    }
                    catch
                    {
                        return (false, $"Sunucu hatası ({(int)response.StatusCode}): {(content.Length > 100 ? content.Substring(0, 100) : content)}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"API'ye bağlanılamadı. Lütfen API'nin çalıştığından emin olun. Hata: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}");
            }
        }

     
        /// Kullanıcının giriş yapıp yapmadığını kontrol eder
     
        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var token = await _tokenService.GetTokenAsync();
                return !string.IsNullOrEmpty(token);
            }
            catch
            {
                return false;
            }
        }

     
        /// Mevcut kullanıcının bilgilerini getirir
     
        public async Task<CurrentUserInfo?> GetCurrentUserAsync()
        {
            try
            {
                var token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                    return null;

                // Token'dan kullanıcı bilgilerini çıkar (JWT decode)
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                
                // API'de "userId" claim'i kullanılıyor
                var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;
                var userTypeClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "userType")?.Value;
                var nameClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "name" || x.Type == "sub")?.Value;

                if (int.TryParse(userIdClaim, out var userId) && !string.IsNullOrEmpty(userTypeClaim))
                {
                    return new CurrentUserInfo
                    {
                        UserId = userId,
                        UserType = userTypeClaim,
                        Name = nameClaim ?? "Kullanıcı"
                    };
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Kullanıcı çıkışı yapar ve cache'i temizler
        /// Requirement 4.2: Logout sırasında membership cache temizlenir
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                // Önce kullanıcı bilgilerini al (cache temizlemek için studentId gerekli)
                var userInfo = await GetCurrentUserAsync();
                
                // Token'ı temizle
                await _tokenService.ClearTokenAsync();
                
                // Membership cache'i temizle (sadece student için)
                if (userInfo != null && userInfo.UserType.Equals("student", StringComparison.OrdinalIgnoreCase))
                {
                    await _membershipCacheService.ClearCacheAsync(userInfo.UserId);
                    Console.WriteLine($"Membership cache temizlendi. StudentID: {userInfo.UserId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout hatası: {ex.Message}");
                // Hata olsa bile token'ı temizlemeye çalış
                try
                {
                    await _tokenService.ClearTokenAsync();
                }
                catch { }
            }
        }
    }

 
    /// Mevcut kullanıcı bilgileri
 
    public class CurrentUserInfo
    {
        public int UserId { get; set; }
        public string UserType { get; set; } = string.Empty; // "Student" veya "Club"
        public string Name { get; set; } = string.Empty;
    }
}
