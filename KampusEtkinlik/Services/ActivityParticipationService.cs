using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace KampusEtkinlik.Services
{
    public class ActivityParticipationService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _configuration;

        public ActivityParticipationService(HttpClient httpClient, TokenService tokenService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7077");
        }

        private async Task SetAuthHeaderAsync()
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// Etkinliğe katılım durumunu kontrol eder
        /// </summary>
        public async Task<ParticipationCheckResult> CheckParticipationAsync(int activityId, int studentId)
        {
            try
            {
                await SetAuthHeaderAsync();
                var response = await _httpClient.GetAsync($"/api/ActivityParticipations/check/{activityId}/{studentId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<ParticipationCheckData>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    return new ParticipationCheckResult
                    {
                        Success = true,
                        IsParticipating = result?.Data?.IsParticipating ?? false,
                        CurrentRating = result?.Data?.Participation?.Rating
                    };
                }
                
                return new ParticipationCheckResult { Success = false, IsParticipating = false };
            }
            catch
            {
                return new ParticipationCheckResult { Success = false, IsParticipating = false };
            }
        }

        /// <summary>
        /// Etkinliğe kayıt olur
        /// </summary>
        public async Task<ParticipationResult> JoinActivityAsync(int activityId)
        {
            try
            {
                await SetAuthHeaderAsync();
                var response = await _httpClient.PostAsync($"/api/ActivityParticipations/join/{activityId}", null);
                var json = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return new ParticipationResult { Success = true, Message = "Etkinliğe başarıyla kayıt oldunuz" };
                }
                
                var errorResult = JsonSerializer.Deserialize<ApiResponse<object>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return new ParticipationResult { Success = false, Message = errorResult?.Message ?? "Kayıt işlemi başarısız" };
            }
            catch (Exception ex)
            {
                return new ParticipationResult { Success = false, Message = $"Hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// Etkinlik kaydını iptal eder
        /// </summary>
        public async Task<ParticipationResult> LeaveActivityAsync(int activityId)
        {
            try
            {
                await SetAuthHeaderAsync();
                var response = await _httpClient.DeleteAsync($"/api/ActivityParticipations/leave/{activityId}");
                var json = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return new ParticipationResult { Success = true, Message = "Etkinlik kaydınız iptal edildi" };
                }
                
                var errorResult = JsonSerializer.Deserialize<ApiResponse<object>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return new ParticipationResult { Success = false, Message = errorResult?.Message ?? "İptal işlemi başarısız" };
            }
            catch (Exception ex)
            {
                return new ParticipationResult { Success = false, Message = $"Hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// Etkinliğe puan verir
        /// </summary>
        public async Task<ParticipationResult> RateActivityAsync(int activityId, int rating)
        {
            try
            {
                await SetAuthHeaderAsync();
                var content = new StringContent(
                    JsonSerializer.Serialize(new { rating }),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PutAsync($"/api/ActivityParticipations/rate/{activityId}", content);
                var json = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return new ParticipationResult { Success = true, Message = "Değerlendirmeniz kaydedildi" };
                }
                
                var errorResult = JsonSerializer.Deserialize<ApiResponse<object>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return new ParticipationResult { Success = false, Message = errorResult?.Message ?? "Değerlendirme başarısız" };
            }
            catch (Exception ex)
            {
                return new ParticipationResult { Success = false, Message = $"Hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// Etkinliğin ortalama puanını getirir
        /// </summary>
        public async Task<ActivityRatingResult> GetActivityRatingAsync(int activityId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/ActivityParticipations/rating/{activityId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<ActivityRatingData>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    return new ActivityRatingResult
                    {
                        Success = true,
                        AverageRating = result?.Data?.AverageRating ?? 0,
                        RatingCount = result?.Data?.RatingCount ?? 0
                    };
                }
                
                return new ActivityRatingResult { Success = false };
            }
            catch
            {
                return new ActivityRatingResult { Success = false };
            }
        }
    }

    public class ParticipationCheckResult
    {
        public bool Success { get; set; }
        public bool IsParticipating { get; set; }
        public int? CurrentRating { get; set; }
    }

    public class ParticipationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ParticipationCheckData
    {
        public bool IsParticipating { get; set; }
        public ParticipationData? Participation { get; set; }
    }

    public class ParticipationData
    {
        public int? Rating { get; set; }
    }

    public class ActivityRatingResult
    {
        public bool Success { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
    }

    public class ActivityRatingData
    {
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
    }
}
