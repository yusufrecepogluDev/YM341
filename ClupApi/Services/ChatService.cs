using ClupApi.DTOs;
using ClupApi.Repositories.Interfaces;
using System.Text;
using System.Text.Json;

namespace ClupApi.Services
{
    /// <summary>
    /// Service for managing chat communication with N8n webhook
    /// Implements retry logic, timeout handling, and HTTPS protocol enforcement
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatService> _logger;
        private readonly string _webhookUrl;
        private readonly int _timeoutSeconds;
        private readonly int _retryCount;
        private readonly string? _apiKey;

        public ChatService(HttpClient httpClient, IConfiguration configuration, ILogger<ChatService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Read N8n settings from configuration
            _webhookUrl = _configuration["N8nSettings:WebhookUrl"] ?? string.Empty;
            _timeoutSeconds = int.TryParse(_configuration["N8nSettings:TimeoutSeconds"], out var timeout) ? timeout : 30;
            _retryCount = int.TryParse(_configuration["N8nSettings:RetryCount"], out var retry) ? retry : 2;
            _apiKey = _configuration["N8nSettings:ApiKey"];

            // Validate webhook URL
            ValidateWebhookUrl();

            // Configure HttpClient timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
        }

        /// <summary>
        /// Validates that the webhook URL is configured and uses HTTPS protocol
        /// </summary>
        private void ValidateWebhookUrl()
        {
            if (string.IsNullOrWhiteSpace(_webhookUrl))
            {
                _logger.LogWarning("N8n webhook URL is not configured. Chat functionality will not work.");
                return;
            }

            // HTTPS protocol enforcement (Requirement 7.1)
            if (!_webhookUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("N8n webhook URL must use HTTPS protocol. Current URL: {WebhookUrl}", _webhookUrl);
                throw new InvalidOperationException("N8n webhook URL must use HTTPS protocol for security.");
            }

            _logger.LogInformation("N8n webhook URL validated successfully: {WebhookUrl}", _webhookUrl);
        }

        /// <summary>
        /// Sends a chat message to N8n webhook and returns the bot response
        /// Implements retry logic and timeout handling
        /// </summary>
        public async Task<ChatResponseDto> SendToN8nAsync(string message, string userId, string? sessionId = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be empty", nameof(message));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(_webhookUrl))
            {
                _logger.LogError("N8n webhook URL is not configured");
                return new ChatResponseDto
                {
                    Success = false,
                    ErrorMessage = "Chat service is not configured. Please contact administrator.",
                    Timestamp = DateTime.UtcNow
                };
            }

            // Prepare request payload (Requirement 4.2 - include user identity)
            var request = new N8nWebhookRequest
            {
                ChatInput = message,
                UserId = userId,
                SessionId = sessionId,
                Timestamp = DateTime.UtcNow
            };

            // Implement retry logic (Requirement 4.4)
            int attempt = 0;
            Exception? lastException = null;

            while (attempt <= _retryCount)
            {
                attempt++;
                try
                {
                    _logger.LogInformation("Sending message to N8n webhook (Attempt {Attempt}/{MaxAttempts})", attempt, _retryCount + 1);

                    var response = await SendRequestAsync(request);
                    
                    _logger.LogInformation("Successfully received response from N8n webhook");
                    
                    return new ChatResponseDto
                    {
                        Response = response.Response,
                        Success = true,
                        Timestamp = DateTime.UtcNow
                    };
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "HTTP request failed on attempt {Attempt}/{MaxAttempts}", attempt, _retryCount + 1);
                    
                    if (attempt <= _retryCount)
                    {
                        // Exponential backoff: wait 2^attempt seconds
                        var delaySeconds = Math.Pow(2, attempt);
                        _logger.LogInformation("Retrying in {Delay} seconds...", delaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    }
                }
                catch (TaskCanceledException ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "Request timeout on attempt {Attempt}/{MaxAttempts}", attempt, _retryCount + 1);
                    
                    if (attempt <= _retryCount)
                    {
                        var delaySeconds = Math.Pow(2, attempt);
                        _logger.LogInformation("Retrying in {Delay} seconds...", delaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogError(ex, "Unexpected error on attempt {Attempt}/{MaxAttempts}", attempt, _retryCount + 1);
                    break; // Don't retry on unexpected errors
                }
            }

            // All retries failed
            _logger.LogError(lastException, "Failed to send message to N8n webhook after {Attempts} attempts", attempt);
            
            return new ChatResponseDto
            {
                Success = false,
                ErrorMessage = lastException is TaskCanceledException 
                    ? "Request timed out. Please try again." 
                    : "Failed to communicate with chat service. Please try again later.",
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Sends HTTP POST request to N8n webhook
        /// </summary>
        private async Task<N8nWebhookResponse> SendRequestAsync(N8nWebhookRequest request)
        {
            var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Add API key header if configured
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            }

            // Send POST request to N8n webhook (Requirement 4.1)
            var httpResponse = await _httpClient.PostAsync(_webhookUrl, httpContent);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError("N8n webhook returned error status {StatusCode}: {ErrorContent}", 
                    httpResponse.StatusCode, errorContent);
                throw new HttpRequestException($"N8n webhook returned status code {httpResponse.StatusCode}");
            }

            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            
            // Validate response format (Requirement 7.5)
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                _logger.LogError("N8n webhook returned empty response");
                throw new InvalidOperationException("N8n webhook returned empty response");
            }

            // Try to extract the AI response from various formats
            string? aiResponse = null;
            
            try
            {
                // First try standard format
                var webhookResponse = JsonSerializer.Deserialize<N8nWebhookResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                });
                
                if (webhookResponse != null && !string.IsNullOrWhiteSpace(webhookResponse.Response))
                {
                    aiResponse = webhookResponse.Response;
                }
            }
            catch (JsonException)
            {
                // Standard format didn't work, try other formats
            }

            // If standard format didn't work, try to extract from LangChain format
            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                try
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    var root = doc.RootElement;
                    
                    // Try to find "output" field (common N8n AI response)
                    if (root.TryGetProperty("output", out var outputElement))
                    {
                        aiResponse = outputElement.GetString();
                    }
                    // Try to find "text" field
                    else if (root.TryGetProperty("text", out var textElement))
                    {
                        aiResponse = textElement.GetString();
                    }
                    // Try to find "content" field
                    else if (root.TryGetProperty("content", out var contentElement))
                    {
                        aiResponse = contentElement.GetString();
                    }
                    // Try to find nested in "message" -> "content"
                    else if (root.TryGetProperty("message", out var messageElement) && 
                             messageElement.TryGetProperty("content", out var msgContentElement))
                    {
                        aiResponse = msgContentElement.GetString();
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse as JSON, trying as plain text");
                }
            }

            // If still no response, check if it's an array (LangChain messages format)
            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                try
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        // Get the last message (AI response)
                        var messages = doc.RootElement.EnumerateArray().ToList();
                        if (messages.Count > 0)
                        {
                            var lastMessage = messages[^1];
                            if (lastMessage.TryGetProperty("kwargs", out var kwargs) &&
                                kwargs.TryGetProperty("content", out var content))
                            {
                                aiResponse = content.GetString();
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Not an array format
                }
            }

            // Last resort: use raw content if it looks like text
            if (string.IsNullOrWhiteSpace(aiResponse) && !responseContent.TrimStart().StartsWith("{") && !responseContent.TrimStart().StartsWith("["))
            {
                aiResponse = responseContent;
            }

            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                _logger.LogError("Could not extract AI response from N8n webhook response: {ResponseContent}", responseContent);
                throw new InvalidOperationException("N8n webhook response format not recognized");
            }

            _logger.LogInformation("Successfully extracted AI response: {Response}", aiResponse.Substring(0, Math.Min(100, aiResponse.Length)));

            return new N8nWebhookResponse { Response = aiResponse };
        }
    }
}
