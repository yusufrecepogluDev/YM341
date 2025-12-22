using System.Net.Http.Json;
using System.Text.Json;
using KampusEtkinlik.Data.Models;

namespace KampusEtkinlik.Services
{
    public class RecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly ActivityService _activityService;
        private readonly string? _n8nWebhookUrl;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(
            HttpClient httpClient,
            ActivityService activityService,
            IConfiguration configuration,
            ILogger<RecommendationService> logger)
        {
            _httpClient = httpClient;
            _activityService = activityService;
            _n8nWebhookUrl = configuration["N8n:WebhookUrl"];
            _logger = logger;

            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<List<Activity>> GetRecommendationsAsync(int studentId)
        {
            if (string.IsNullOrEmpty(_n8nWebhookUrl))
            {
                _logger.LogWarning("n8n webhook URL is not configured");
                return new List<Activity>();
            }

            try
            {
                var request = new { studentId };
                var response = await _httpClient.PostAsJsonAsync(_n8nWebhookUrl, request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("n8n webhook returned status {StatusCode}", response.StatusCode);
                    return new List<Activity>();
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("n8n response: {Content}", content);
                
                List<int>? recommendedIds = null;
                
                // Önce direkt format dene: {"recommendedActivityIds": [...]}
                try
                {
                    var directResponse = JsonSerializer.Deserialize<RecommendationResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    recommendedIds = directResponse?.RecommendedActivityIds;
                }
                catch { }
                
                // Eğer boşsa, wrapper format dene: {"output":{"recommendedActivityIds": [...]}}
                if (recommendedIds == null || !recommendedIds.Any())
                {
                    try
                    {
                        var n8nResponse = JsonSerializer.Deserialize<N8nRecommendationResponse>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        recommendedIds = n8nResponse?.Output?.RecommendedActivityIds;
                    }
                    catch { }
                }

                if (recommendedIds == null || !recommendedIds.Any())
                {
                    _logger.LogWarning("No recommendations found in response");
                    return new List<Activity>();
                }
                
                _logger.LogInformation("Recommended IDs: {Ids}", string.Join(", ", recommendedIds));

                var allActivities = await _activityService.GetAllAsync();
                var recommendedActivities = allActivities
                    .Where(a => recommendedIds.Contains(a.ActivityID))
                    .Take(5)
                    .ToList();

                return recommendedActivities;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("n8n webhook request timed out");
                return new List<Activity>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to n8n webhook");
                return new List<Activity>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse n8n response");
                return new List<Activity>();
            }
        }
    }
}
