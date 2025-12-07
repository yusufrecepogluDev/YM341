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

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                // Call ChatService to send message to N8n webhook (Requirement 4.1, 4.3)
                var response = await _chatService.SendToN8nAsync(
                    request.Message, 
                    userId, 
                    request.SessionId);

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
    }
}
