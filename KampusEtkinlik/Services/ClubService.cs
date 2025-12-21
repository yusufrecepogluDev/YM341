using System.Net.Http.Json;
using System.Net.Http.Headers;
using KampusEtkinlik.Data.Models;
using KampusEtkinlik.Data.DTOs;
using Microsoft.Extensions.Configuration;

namespace KampusEtkinlik.Services
{
    public class ClubService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;
        private readonly string _baseUrl;

        public ClubService(HttpClient httpClient, TokenService tokenService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _baseUrl = $"{configuration["ApiSettings:BaseUrl"]}/api/Clubs";
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

 
        /// Tüm aktif kulüpleri getirir
 
        public async Task<List<ClubResponseDto>> GetAllAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<ClubResponseDto>>>(_baseUrl);
                return result?.Data ?? new List<ClubResponseDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API bağlantı hatası: {ex.Message}");
                return new List<ClubResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
                return new List<ClubResponseDto>();
            }
        }

 
        /// Belirli bir kulübün detaylarını getirir
 
        public async Task<ClubResponseDto?> GetByIdAsync(int id)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var result = await _httpClient.GetFromJsonAsync<ApiResponse<ClubResponseDto>>($"{_baseUrl}/{id}");
                return result?.Data;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API bağlantı hatası: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
                return null;
            }
        }

 
        /// Kulüp oluşturur (sadece kulüp hesapları için)
 
        public async Task<(bool Success, string Message)> CreateAsync(ClubCreateDto createDto)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.PostAsJsonAsync(_baseUrl, createDto);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClubResponseDto>>();
                    return (true, result?.Message ?? "Kulüp başarıyla oluşturuldu.");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                    return (false, errorResult?.Message ?? "Kulüp oluşturulamadı.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kulüp oluşturma hatası: {ex.Message}");
                return (false, "Kulüp oluşturma sırasında bir hata oluştu.");
            }
        }

 
        /// Kulüp bilgilerini günceller (sadece kulüp hesapları için)
 
        public async Task<(bool Success, string Message)> UpdateAsync(int id, ClubUpdateDto updateDto)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{id}", updateDto);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<ClubResponseDto>>();
                    return (true, result?.Message ?? "Kulüp bilgileri güncellendi.");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                    return (false, errorResult?.Message ?? "Kulüp bilgileri güncellenemedi.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kulüp güncelleme hatası: {ex.Message}");
                return (false, "Kulüp güncelleme sırasında bir hata oluştu.");
            }
        }

 
        /// Kulübü siler (sadece kulüp hesapları için)
 
        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Kulüp başarıyla silindi.");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                    return (false, errorResult?.Message ?? "Kulüp silinemedi.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kulüp silme hatası: {ex.Message}");
                return (false, "Kulüp silme sırasında bir hata oluştu.");
            }
        }
    }

 
    /// Kulüp response DTO'su
 
    public class ClubResponseDto
    {
        public int ClubID { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long ClubNumber { get; set; }
        public bool IsActive { get; set; }
        public int MemberCount { get; set; }
        public int AnnouncementCount { get; set; }
        public int ActivityCount { get; set; }
    }

 
    /// Kulüp oluşturma DTO'su
 
    public class ClubCreateDto
    {
        public string ClubName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long ClubNumber { get; set; }
        public string ClubPassword { get; set; } = string.Empty;
    }

 
    /// Kulüp güncelleme DTO'su
 
    public class ClubUpdateDto
    {
        public string ClubName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ClubPassword { get; set; }
        public bool? IsActive { get; set; }
    }
}