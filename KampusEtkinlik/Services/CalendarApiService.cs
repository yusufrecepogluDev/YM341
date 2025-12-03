using System.Net.Http.Json;
using KampusEtkinlik.Data.Models;
using Microsoft.Extensions.Configuration;

namespace KampusEtkinlik.Services
{
    public class CalendarApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public CalendarApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = $"{configuration["ApiSettings:BaseUrl"]}/api/Calendar";
        }

        public async Task<List<CalendarEvent>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var url = $"{_baseUrl}/events?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CalendarEvent>>>(url);
                return response?.Data ?? new List<CalendarEvent>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API bağlantı hatası: {ex.Message}");
                return new List<CalendarEvent>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
                return new List<CalendarEvent>();
            }
        }

        public async Task<List<CalendarEvent>> GetDailyEventsAsync(DateTime date)
        {
            try
            {
                var url = $"{_baseUrl}/events/daily?date={date:yyyy-MM-dd}";
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CalendarEvent>>>(url);
                return response?.Data ?? new List<CalendarEvent>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API bağlantı hatası: {ex.Message}");
                return new List<CalendarEvent>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
                return new List<CalendarEvent>();
            }
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                var url = $"{_baseUrl}/categories";
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Category>>>(url);
                return response?.Data ?? new List<Category>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API bağlantı hatası: {ex.Message}");
                return new List<Category>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen hata: {ex.Message}");
                return new List<Category>();
            }
        }
    }
}
