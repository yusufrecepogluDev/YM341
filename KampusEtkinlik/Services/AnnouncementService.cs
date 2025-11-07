using System.Net.Http.Json;
using KampusEtkinlik.Data.Models;

namespace KampusEtkinlik.Services
{
    public class AnnouncementService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:5245/api/Announcements";

        public AnnouncementService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Announcement>> GetAllAsync()
        {
            try
            {
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
            return await _httpClient.GetFromJsonAsync<Announcement>($"{_baseUrl}/{id}");
        }

        public async Task<bool> AddAsync(Announcement announcement)
        {
            var response = await _httpClient.PostAsJsonAsync(_baseUrl, announcement);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Announcement announcement)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{announcement.AnnouncementID}", announcement);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
