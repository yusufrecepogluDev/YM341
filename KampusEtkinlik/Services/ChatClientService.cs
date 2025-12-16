using KampusEtkinlik.Data.DTOs;
using KampusEtkinlik.Data.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http;

namespace KampusEtkinlik.Services
{
    /// <summary>
    /// Interface for chat client service
    /// </summary>
    public interface IChatClientService
    {
        Task<ChatMessageDto?> SendMessageAsync(string message);
        Task<List<ChatMessageDto>> LoadHistoryAsync();
        Task SaveHistoryAsync(List<ChatMessageDto> messages);
        Task ClearHistoryAsync();
        Task InitializeChatAsync();
    }

    /// <summary>
    /// Service for managing chat communication with backend API
    /// Handles message sending, history persistence, and session storage
    /// </summary>
    public class ChatClientService : IChatClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly TokenService _tokenService;
        private readonly ILogger<ChatClientService> _logger;
        private readonly string _baseUrl;
        private const string ChatHistoryKey = "chatHistory";
        private const string SessionIdKey = "chatSessionId";
        private const int MaxHistorySize = 50;
        private string? _cachedSessionId;

        public ChatClientService(
            IHttpClientFactory httpClientFactory,
            ProtectedSessionStorage sessionStorage,
            TokenService tokenService,
            ILogger<ChatClientService> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _sessionStorage = sessionStorage ?? throw new ArgumentNullException(nameof(sessionStorage));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseUrl = $"{configuration["ApiSettings:BaseUrl"]}/api/chat";
        }

        /// <summary>
        /// Sends a message to the backend chat API and returns the bot's response
        /// Requirement 2.2: Send message to ClupApi
        /// </summary>
        public async Task<ChatMessageDto?> SendMessageAsync(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    _logger.LogWarning("Attempted to send empty message");
                    return null;
                }

                // Get authentication token
                var token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("No authentication token found");
                    throw new UnauthorizedAccessException("Kullanıcı oturumu bulunamadı");
                }

                _logger.LogInformation("Token retrieved, length: {Length}", token.Length);
                
                // Check if token is expired
                if (_tokenService.IsTokenExpired(token))
                {
                    _logger.LogError("Token is expired");
                    throw new UnauthorizedAccessException("Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.");
                }

                // Prepare request
                var request = new ChatRequestDto
                {
                    Message = message,
                    SessionId = await GetOrCreateSessionId() // Use consistent session ID
                };

                _logger.LogInformation("Sending message to backend API");

                // Create a new HttpClient for this request using named client
                var httpClient = _httpClientFactory.CreateClient("ChatClient");
                
                // Create request message manually to ensure headers are preserved
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/message");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                requestMessage.Content = JsonContent.Create(request);

                _logger.LogInformation("Sending POST request to: {Url}", _baseUrl + "/message");
                
                // Send request to backend
                var response = await httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Backend returned error: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    
                    throw new HttpRequestException($"Backend error: {response.StatusCode} - {errorContent}");
                }

                // Parse response
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ChatResponseDto>>();

                if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
                {
                    _logger.LogError("Invalid response from backend");
                    throw new InvalidOperationException("Geçersiz yanıt alındı");
                }

                // Create bot message
                var botMessage = new ChatMessageDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = apiResponse.Data.Response,
                    IsBot = true,
                    Timestamp = apiResponse.Data.Timestamp,
                    Status = MessageStatus.Delivered
                };

                _logger.LogInformation("Successfully received bot response");

                return botMessage;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while sending message");
                throw new Exception("Bağlantı hatası. Lütfen internet bağlantınızı kontrol edin.", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while sending message");
                throw new Exception("İstek zaman aşımına uğradı. Lütfen tekrar deneyin.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending message");
                throw;
            }
        }

        /// <summary>
        /// Loads chat history from session storage
        /// Requirement 6.2: Load history from session storage
        /// </summary>
        public async Task<List<ChatMessageDto>> LoadHistoryAsync()
        {
            try
            {
                _logger.LogInformation("Loading chat history from session storage");

                var result = await _sessionStorage.GetAsync<List<ChatMessageDto>>(ChatHistoryKey);

                if (result.Success && result.Value != null)
                {
                    _logger.LogInformation("Loaded {Count} messages from history", result.Value.Count);
                    return result.Value;
                }

                _logger.LogInformation("No chat history found");
                return new List<ChatMessageDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat history");
                return new List<ChatMessageDto>();
            }
        }

        /// <summary>
        /// Saves chat history to session storage
        /// Requirement 6.1: Save messages to session storage
        /// Requirement 6.4: Limit to 50 messages
        /// </summary>
        public async Task SaveHistoryAsync(List<ChatMessageDto> messages)
        {
            try
            {
                if (messages == null)
                {
                    _logger.LogWarning("Attempted to save null message list");
                    return;
                }

                // Limit to 50 messages (keep most recent)
                var messagesToSave = messages.Count > MaxHistorySize
                    ? messages.Skip(messages.Count - MaxHistorySize).ToList()
                    : messages;

                _logger.LogInformation("Saving {Count} messages to session storage", messagesToSave.Count);

                await _sessionStorage.SetAsync(ChatHistoryKey, messagesToSave);

                _logger.LogInformation("Chat history saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving chat history");
                // Don't throw - saving history failure shouldn't break the app
            }
        }

        /// <summary>
        /// Clears all chat history from session storage
        /// Requirement 6.5: Clear history functionality
        /// </summary>
        public async Task ClearHistoryAsync()
        {
            try
            {
                _logger.LogInformation("Clearing chat history");

                await _sessionStorage.DeleteAsync(ChatHistoryKey);

                _logger.LogInformation("Chat history cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing chat history");
                throw;
            }
        }

        /// <summary>
        /// Gets or creates a session ID for the chat
        /// </summary>
        private async Task<string> GetOrCreateSessionId()
        {
            // Return cached session ID if available
            if (!string.IsNullOrEmpty(_cachedSessionId))
            {
                return _cachedSessionId;
            }

            try
            {
                // Try to load from session storage
                var result = await _sessionStorage.GetAsync<string>(SessionIdKey);
                if (result.Success && !string.IsNullOrEmpty(result.Value))
                {
                    _cachedSessionId = result.Value;
                    _logger.LogInformation("Loaded existing session ID from storage");
                    return _cachedSessionId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading session ID from storage");
            }

            // Create new session ID
            _cachedSessionId = Guid.NewGuid().ToString();
            _logger.LogInformation("Created new session ID: {SessionId}", _cachedSessionId);

            try
            {
                // Save to session storage
                await _sessionStorage.SetAsync(SessionIdKey, _cachedSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error saving session ID to storage");
            }

            return _cachedSessionId;
        }

        /// <summary>
        /// Initializes chat session by sending calendar context to N8n
        /// This is called when chat widget is opened
        /// </summary>
        public async Task InitializeChatAsync()
        {
            try
            {
                _logger.LogInformation("Initializing chat session");

                // Get or create session ID
                var sessionId = await GetOrCreateSessionId();

                // Get token
                var token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No token available for chat initialization");
                    return;
                }

                _logger.LogInformation("Token retrieved, length: {Length}", token.Length);

                // Create HTTP client
                var client = _httpClientFactory.CreateClient("ChatClient");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Prepare request
                var request = new
                {
                    Message = "initialize",
                    SessionId = sessionId
                };

                _logger.LogInformation("Sending initialization request to backend API");

                // Send initialization request
                var response = await client.PostAsJsonAsync("/api/chat/initialize", request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Chat session initialized successfully");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Chat initialization returned {StatusCode}: {Error}", 
                        response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing chat session");
                // Don't throw - initialization failure shouldn't break the app
            }
        }
    }
}
