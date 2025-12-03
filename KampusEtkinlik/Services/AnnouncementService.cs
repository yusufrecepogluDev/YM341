using System.Net.Http.Json;
using System.Net.Http.Headers;
using KampusEtkinlik.Data.Models;
using KampusEtkinlik.Data.DTOs;
using Microsoft.Extensions.Configuration;

namespace KampusEtkinlik.Services
{
    public class AnnouncementService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;
        private readonly string _baseUrl;

        public AnnouncementService(HttpClient httpClient, TokenService tokenService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _baseUrl = $"{configuration["ApiSettings:BaseUrl"]}/api/Announcements";
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Announcement>> GetAllAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var result = await _httpClient.GetFromJsonAsync<List<Announcement>>(_baseUrl);
                return result ?? new List<Announcement>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API bağlantı hatası: {ex.Message}");
                return new List<Announcement>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
                return new List<Announcement>();
            }
        }

        public async Task<Announcement?> GetByIdAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<Announcement>($"{_baseUrl}/{id}");
        }

        public async Task<bool> AddAsync(AnnouncementCreateDto createDto)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.PostAsJsonAsync(_baseUrl, createDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Duyuru oluşturma hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(int id, Announcement announcement)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                
                var updateDto = new
                {
                    AnnouncementTitle = announcement.AnnouncementTitle,
                    AnnouncementContent = announcement.AnnouncementContent,
                    StartDate = announcement.StartDate
                };
                
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{id}", updateDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Duyuru güncelleme hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Duyuru silme hatası: {ex.Message}");
                return false;
            }
        }
    }
}
