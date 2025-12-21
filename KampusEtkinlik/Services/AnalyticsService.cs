using System.Net.Http.Headers;
using System.Text.Json;
using KampusEtkinlik.Data.Models;

namespace KampusEtkinlik.Services
{
    public class AnalyticsService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;
        private readonly JsonSerializerOptions _jsonOptions;

        public AnalyticsService(HttpClient httpClient, TokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private async Task SetAuthHeaderAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<ClubAnalyticsSummaryDto?> GetClubAnalyticsAsync(int clubId)
        {
            try
            {
                await SetAuthHeaderAsync();
                var response = await _httpClient.GetAsync($"api/Analytics/club/{clubId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ClubAnalyticsSummaryDto>>(content, _jsonOptions);
                    return apiResponse?.Data;
                }

                System.Diagnostics.Debug.WriteLine($"Analytics API hatası: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetClubAnalyticsAsync hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<ActivityAnalyticsDto?> GetActivityAnalyticsAsync(int activityId)
        {
            try
            {
                await SetAuthHeaderAsync();
                var response = await _httpClient.GetAsync($"api/Analytics/activity/{activityId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ActivityAnalyticsDto>>(content, _jsonOptions);
                    return apiResponse?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetActivityAnalyticsAsync hatası: {ex.Message}");
                return null;
            }
        }

        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public T? Data { get; set; }
            public string? Message { get; set; }
        }
    }
}
