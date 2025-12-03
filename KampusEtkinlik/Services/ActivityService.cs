using System.Net.Http.Json;
using System.Net.Http.Headers;
using KampusEtkinlik.Data.Models;
using KampusEtkinlik.Data.DTOs;
using Microsoft.Extensions.Configuration;

namespace KampusEtkinlik.Services
{
    public class ActivityService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;
        private readonly string _baseUrl;

        public ActivityService(HttpClient httpClient, TokenService tokenService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _baseUrl = $"{configuration["ApiSettings:BaseUrl"]}/api/Activities";
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Activity>> GetAllAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var result = await _httpClient.GetFromJsonAsync<List<Activity>>(_baseUrl);
                return result ?? new List<Activity>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API bağlantı hatası: {ex.Message}");
                return new List<Activity>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
                return new List<Activity>();
            }
        }

        public async Task<Activity?> GetByIdAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<Activity>($"{_baseUrl}/{id}");
        }

        public async Task<bool> AddAsync(ActivityCreateDto createDto)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.PostAsJsonAsync(_baseUrl, createDto);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Hatası: {error}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ekleme hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(int id, ActivityUpdateDto updateDto)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{id}", updateDto);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Hatası: {error}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Güncelleme hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
