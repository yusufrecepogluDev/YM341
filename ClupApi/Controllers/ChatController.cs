using ClupApi.DTOs;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClupApi.Controllers
{
    /// <summary>
    /// Controller for managing chat interactions with N8n chatbot
    /// Requires authentication via JWT token
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly IConfiguration _configuration;
        
        // Static dictionary to track message counts per session (Requirement 10.1)
        private static readonly Dictionary<string, int> _userMessageCounters = new();
        private static readonly object _counterLock = new();

        public ChatController(IChatService chatService, ILogger<ChatController> logger, IConfiguration configuration)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Sends a message to the N8n chatbot and returns the response
        /// POST /api/chat/message
        /// Requires: JWT authentication
        /// </summary>
        /// <param name="request">Chat request containing the user's message</param>
        /// <returns>ApiResponse with bot's response</returns>
        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
        {
            try
            {
                // Validate model state (Requirement 4.5)
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state in SendMessage request");
                    return HandleValidationErrors(ModelState);
                }

                // Extract user ID from JWT token (Requirement 7.2, 7.3)
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("SendMessage called without valid user ID in JWT token");
                    return Unauthorized(ApiResponse.ErrorResponse(
                        "Geçersiz kimlik doğrulama",
                        new[] { "Kullanıcı kimliği bulunamadı" }));
                }

                _logger.LogInformation("Sending message to N8n chatbot for user {UserId}", userId);

                // Increment message counter (Requirement 10.1)
                if (!string.IsNullOrWhiteSpace(request.SessionId))
                {
                    IncrementMessageCounter(request.SessionId);
                }

                // Check if we should send context data (Requirement 10.1)
                string? contextData = null;
                if (!string.IsNullOrWhiteSpace(request.SessionId) && ShouldSendContext(request.SessionId))
                {
                    _logger.LogInformation("Retrieving calendar context data for session {SessionId}", request.SessionId);
                    
                    try
                    {
                        var calendarContext = await _chatService.GetCalendarContextAsync();
                        contextData = _chatService.FormatContextData(calendarContext);
                        
                        _logger.LogInformation("Calendar context data prepared: {EventCount} events",
                            calendarContext.CalendarEvents.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to retrieve calendar context data, continuing without context");
                        // Continue without context data if retrieval fails
                    }
                }

                // Call ChatService to send message to N8n webhook (Requirement 4.1, 4.3)
                var response = await _chatService.SendToN8nAsync(
                    request.Message, 
                    userId, 
                    request.SessionId,
                    contextData);

                // Handle unsuccessful response (Requirement 4.4, 4.5)
                if (!response.Success)
                {
                    _logger.LogError("Failed to get response from N8n chatbot: {ErrorMessage}", response.ErrorMessage);
                    
                    // Check if it's a timeout error
                    if (response.ErrorMessage?.Contains("timeout", StringComparison.OrdinalIgnoreCase) == true ||
                        response.ErrorMessage?.Contains("timed out", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return StatusCode(504, ApiResponse.ErrorResponse(
                            "İstek zaman aşımına uğradı",
                            new[] { "Chatbot yanıt vermedi. Lütfen tekrar deneyin." }));
                    }
                    
                    // Check if it's a network error
                    if (response.ErrorMessage?.Contains("network", StringComparison.OrdinalIgnoreCase) == true ||
                        response.ErrorMessage?.Contains("connection", StringComparison.OrdinalIgnoreCase) == true ||
                        response.ErrorMessage?.Contains("communicate", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return StatusCode(503, ApiResponse.ErrorResponse(
                            "Bağlantı hatası",
                            new[] { "Chatbot servisine ulaşılamıyor. Lütfen daha sonra tekrar deneyin." }));
                    }

                    // Generic error
                    return StatusCode(500, ApiResponse.ErrorResponse(
                        "Chatbot yanıt veremedi",
                        new[] { response.ErrorMessage ?? "Bilinmeyen hata" }));
                }

                _logger.LogInformation("Successfully received response from N8n chatbot for user {UserId}", userId);

                return Ok(ApiResponse<ChatResponseDto>.SuccessResponse(
                    response,
                    "Mesaj başarıyla gönderildi"));
            }
            catch (ArgumentException ex)
            {
                // Handle validation errors (Requirement 4.5)
                _logger.LogWarning(ex, "Validation error in SendMessage endpoint");
                return BadRequest(ApiResponse.ErrorResponse(
                    "Geçersiz istek",
                    new[] { ex.Message }));
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle authorization errors (Requirement 7.2)
                _logger.LogWarning(ex, "Unauthorized access in SendMessage endpoint");
                return Unauthorized(ApiResponse.ErrorResponse(
                    "Yetkisiz erişim",
                    new[] { ex.Message }));
            }
            catch (TaskCanceledException ex)
            {
                // Handle timeout errors (Requirement 4.4)
                _logger.LogError(ex, "Timeout in SendMessage endpoint");
                return StatusCode(504, ApiResponse.ErrorResponse(
                    "İstek zaman aşımına uğradı",
                    new[] { "Chatbot yanıt vermedi. Lütfen tekrar deneyin." }));
            }
            catch (HttpRequestException ex)
            {
                // Handle network errors (Requirement 4.4)
                _logger.LogError(ex, "Network error in SendMessage endpoint");
                return StatusCode(503, ApiResponse.ErrorResponse(
                    "Bağlantı hatası",
                    new[] { "Chatbot servisine ulaşılamıyor. Lütfen daha sonra tekrar deneyin." }));
            }
            catch (Exception ex)
            {
                // Handle unexpected errors (Requirement 4.5)
                _logger.LogError(ex, "Unexpected error in SendMessage endpoint");
                return HandleInternalServerError("Mesaj gönderilirken bir hata oluştu");
            }
        }

        /// <summary>
        /// Health check endpoint to verify chat service availability
        /// GET /api/chat/health
        /// </summary>
        /// <returns>ApiResponse indicating service health status</returns>
        [HttpGet("health")]
        [AllowAnonymous] // Health check doesn't require authentication
        public IActionResult HealthCheck()
        {
            try
            {
                _logger.LogInformation("Health check requested");

                var healthStatus = new
                {
                    Status = "Healthy",
                    Service = "ChatService",
                    Timestamp = DateTime.UtcNow,
                    Message = "Chat service is operational"
                };

                return Ok(ApiResponse<object>.SuccessResponse(
                    healthStatus,
                    "Servis çalışıyor"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, ApiResponse.ErrorResponse(
                    "Servis sağlık kontrolü başarısız",
                    new[] { ex.Message }));
            }
        }

        /// <summary>
        /// Initializes chat session by sending calendar context to N8n
        /// POST /api/chat/initialize
        /// Requires: JWT authentication
        /// </summary>
        /// <param name="request">Chat request with session ID</param>
        /// <returns>ApiResponse indicating initialization success</returns>
        [HttpPost("initialize")]
        public async Task<IActionResult> InitializeChat([FromBody] ChatRequestDto request)
        {
            try
            {
                // Extract user ID from JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("InitializeChat called without valid user ID in JWT token");
                    return Unauthorized(ApiResponse.ErrorResponse(
                        "Geçersiz kimlik doğrulama",
                        new[] { "Kullanıcı kimliği bulunamadı" }));
                }

                _logger.LogInformation("Initializing chat session for user {UserId}", userId);

                // Get calendar context data
                var calendarContext = await _chatService.GetCalendarContextAsync();
                var contextData = _chatService.FormatContextData(calendarContext);
                
                _logger.LogInformation("Calendar context prepared for initialization: {EventCount} events",
                    calendarContext.CalendarEvents.Count);

                // Send initialization message to N8n with context data
                var initMessage = "Sistem başlatılıyor. Takvim bilgileri yükleniyor.";
                var response = await _chatService.SendToN8nAsync(
                    initMessage, 
                    userId, 
                    request.SessionId,
                    contextData);

                if (!response.Success)
                {
                    _logger.LogError("Failed to initialize chat session: {ErrorMessage}", response.ErrorMessage);
                    return StatusCode(500, ApiResponse.ErrorResponse(
                        "Chat başlatılamadı",
                        new[] { response.ErrorMessage ?? "Bilinmeyen hata" }));
                }

                _logger.LogInformation("Chat session initialized successfully for user {UserId}", userId);

                return Ok(ApiResponse<object>.SuccessResponse(
                    new { Initialized = true, Timestamp = DateTime.UtcNow },
                    "Chat başlatıldı"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in InitializeChat endpoint");
                return HandleInternalServerError("Chat başlatılırken bir hata oluştu");
            }
        }

        /// <summary>
        /// Checks if context data should be sent based on message count
        /// Requirement 10.1: Send context every 14 messages (and on first message)
        /// </summary>
        /// <param name="sessionId">The session ID to check</param>
        /// <returns>True if context should be sent, false otherwise</returns>
        private bool ShouldSendContext(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return false;
            }

            lock (_counterLock)
            {
                if (!_userMessageCounters.TryGetValue(sessionId, out var count))
                {
                    return false;
                }

                // Get context refresh interval from configuration (default: 14)
                var contextRefreshInterval = _configuration.GetValue<int>("N8nSettings:ContextRefreshInterval", 14);

                // Send context on first message (count == 1) and every N messages thereafter
                return count == 1 || count % contextRefreshInterval == 0;
            }
        }

        /// <summary>
        /// Increments the message counter for a session
        /// Requirement 10.1: Track message count per session
        /// </summary>
        /// <param name="sessionId">The session ID to increment</param>
        private void IncrementMessageCounter(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return;
            }

            lock (_counterLock)
            {
                if (_userMessageCounters.ContainsKey(sessionId))
                {
                    _userMessageCounters[sessionId]++;
                }
                else
                {
                    _userMessageCounters[sessionId] = 1;
                }

                _logger.LogDebug("Message counter for session {SessionId}: {Count}", 
                    sessionId, _userMessageCounters[sessionId]);
            }
        }
    }
}
