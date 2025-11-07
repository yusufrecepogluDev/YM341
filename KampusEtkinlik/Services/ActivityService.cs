using System.Net.Http.Json;
using KampusEtkinlik.Data.Models;
using KampusEtkinlik.Data.DTOs;

namespace KampusEtkinlik.Services
{
    public class ActivityService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:5245/api/Activities";

        public ActivityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Activity>> GetAllAsync()
        {
            try
            {
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
            return await _httpClient.GetFromJsonAsync<Activity>($"{_baseUrl}/{id}");
        }

        public async Task<bool> AddAsync(ActivityCreateDto createDto)
        {
            try
            {
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
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
