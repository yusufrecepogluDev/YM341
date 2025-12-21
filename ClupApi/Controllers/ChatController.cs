using ClupApi.DTOs;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClupApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly IConfiguration _configuration;
        
        // Oturum başına mesaj sayısını takip et
        private static readonly Dictionary<string, int> _userMessageCounters = new();
        private static readonly object _counterLock = new();

        public ChatController(IChatService chatService, ILogger<ChatController> logger, IConfiguration configuration)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state in SendMessage request");
                    return HandleValidationErrors(ModelState);
                }

                // JWT token'dan kullanıcı ID'sini al
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("SendMessage called without valid user ID in JWT token");
                    return Unauthorized(ApiResponse.ErrorResponse(
                        "Geçersiz kimlik doğrulama",
                        new[] { "Kullanıcı kimliği bulunamadı" }));
                }

                _logger.LogInformation("Sending message to N8n chatbot for user {UserId}", userId);

                // Mesaj sayacını artır
                if (!string.IsNullOrWhiteSpace(request.SessionId))
                {
                    IncrementMessageCounter(request.SessionId);
                }

                // Bağlam verisi gönderilmeli mi kontrol et
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
                    }
                }

                // N8n webhook'a mesaj gönder
                var response = await _chatService.SendToN8nAsync(
                    request.Message, 
                    userId, 
                    request.SessionId,
                    contextData);

                if (!response.Success)
                {
                    _logger.LogError("Failed to get response from N8n chatbot: {ErrorMessage}", response.ErrorMessage);
                    
                    if (response.ErrorMessage?.Contains("timeout", StringComparison.OrdinalIgnoreCase) == true ||
                        response.ErrorMessage?.Contains("timed out", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return StatusCode(504, ApiResponse.ErrorResponse(
                            "İstek zaman aşımına uğradı",
                            new[] { "Chatbot yanıt vermedi. Lütfen tekrar deneyin." }));
                    }
                    
                    if (response.ErrorMessage?.Contains("network", StringComparison.OrdinalIgnoreCase) == true ||
                        response.ErrorMessage?.Contains("connection", StringComparison.OrdinalIgnoreCase) == true ||
                        response.ErrorMessage?.Contains("communicate", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return StatusCode(503, ApiResponse.ErrorResponse(
                            "Bağlantı hatası",
                            new[] { "Chatbot servisine ulaşılamıyor. Lütfen daha sonra tekrar deneyin." }));
                    }

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
                _logger.LogWarning(ex, "Validation error in SendMessage endpoint");
                return BadRequest(ApiResponse.ErrorResponse(
                    "Geçersiz istek",
                    new[] { ex.Message }));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in SendMessage endpoint");
                return Unauthorized(ApiResponse.ErrorResponse(
                    "Yetkisiz erişim",
                    new[] { ex.Message }));
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout in SendMessage endpoint");
                return StatusCode(504, ApiResponse.ErrorResponse(
                    "İstek zaman aşımına uğradı",
                    new[] { "Chatbot yanıt vermedi. Lütfen tekrar deneyin." }));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error in SendMessage endpoint");
                return StatusCode(503, ApiResponse.ErrorResponse(
                    "Bağlantı hatası",
                    new[] { "Chatbot servisine ulaşılamıyor. Lütfen daha sonra tekrar deneyin." }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SendMessage endpoint");
                return HandleInternalServerError("Mesaj gönderilirken bir hata oluştu");
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
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

        [HttpPost("initialize")]
        public async Task<IActionResult> InitializeChat([FromBody] ChatRequestDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("InitializeChat called without valid user ID in JWT token");
                    return Unauthorized(ApiResponse.ErrorResponse(
                        "Geçersiz kimlik doğrulama",
                        new[] { "Kullanıcı kimliği bulunamadı" }));
                }

                _logger.LogInformation("Initializing chat session for user {UserId}", userId);

                // Takvim bağlam verisini al
                var calendarContext = await _chatService.GetCalendarContextAsync();
                var contextData = _chatService.FormatContextData(calendarContext);
                
                _logger.LogInformation("Calendar context prepared for initialization: {EventCount} events",
                    calendarContext.CalendarEvents.Count);

                // N8n'e başlatma mesajı gönder
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

        // Her 14 mesajda bir bağlam gönder
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

        // Oturum verilerini temizle
        public static void ClearSessionData(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return;
            }

            lock (_counterLock)
            {
                _userMessageCounters.Remove(sessionId);
            }
        }
    }
}
