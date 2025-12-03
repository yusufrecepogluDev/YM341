using System.Net.Http.Json;
using System.Net.Http.Headers;
using KampusEtkinlik.Data.Models;
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

        public async Task<bool> AddAsync(Announcement announcement)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync(_baseUrl, announcement);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Announcement announcement)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{announcement.AnnouncementID}", announcement);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
