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

        public AuthenticationService(HttpClient httpClient, IConfiguration configuration, TokenService tokenService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _tokenService = tokenService;
            
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            _httpClient.BaseAddress = new Uri(baseUrl ?? "https://localhost:7077");
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

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<StudentLoginResponseDto>>();
                    return (true, result?.Data, result?.Message ?? "Giriş başarılı");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return (false, null, errorResult?.Message ?? "Giriş başarısız");
                }
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

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClubLoginResponseDto>>();
                    return (true, result?.Data, result?.Message ?? "Giriş başarılı");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return (false, null, errorResult?.Message ?? "Giriş başarısız");
                }
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

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return (true, result?.Message ?? "Kayıt başarılı");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return (false, errorResult?.Message ?? "Kayıt başarısız");
                }
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

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return (true, result?.Message ?? "Kayıt başarılı");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    return (false, errorResult?.Message ?? "Kayıt başarısız");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}");
            }
        }
    }
}
