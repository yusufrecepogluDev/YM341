using System.ComponentModel.DataAnnotations;

namespace ClupApi.DTOs
{
    /// <summary>
    /// Represents a single chat message in the conversation
    /// </summary>
    public class ChatMessageDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required(ErrorMessage = "Mesaj içeriği zorunludur")]
        public string Content { get; set; } = string.Empty;
        
        public bool IsBot { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public MessageStatus Status { get; set; } = MessageStatus.Sent;
    }

    /// <summary>
    /// Status of a chat message
    /// </summary>
    public enum MessageStatus
    {
        Sent,
        Delivered,
        Error
    }

    /// <summary>
    /// Request DTO for sending a chat message
    /// </summary>
    public class ChatRequestDto
    {
        [Required(ErrorMessage = "Mesaj zorunludur")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Mesaj 1 ile 500 karakter arasında olmalıdır")]
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional session ID for maintaining conversation context
        /// </summary>
        public string? SessionId { get; set; }
    }

    /// <summary>
    /// Response DTO from chat service
    /// </summary>
    public class ChatResponseDto
    {
        [Required(ErrorMessage = "Yanıt zorunludur")]
        public string Response { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public bool Success { get; set; } = true;
        
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Request payload sent to N8n webhook
    /// </summary>
    public class N8nWebhookRequest
    {
        /// <summary>
        /// Chat input message - named 'chatInput' for N8n AI Agent compatibility
        /// </summary>
        [Required(ErrorMessage = "Mesaj zorunludur")]
        public string ChatInput { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Kullanıcı ID'si zorunludur")]
        public string UserId { get; set; } = string.Empty;
        
        public string? SessionId { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Context data containing upcoming activities and announcements (sent every 14 messages)
        /// </summary>
        public string? ContextData { get; set; }
    }

    /// <summary>
    /// Response payload received from N8n webhook
    /// </summary>
    public class N8nWebhookResponse
    {
        [Required(ErrorMessage = "Yanıt zorunludur")]
        public string Response { get; set; } = string.Empty;
        
        public string? SessionId { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Calendar context data - uses same CalendarEventDto as calendar page
    /// </summary>
    public class CalendarContextDto
    {
        public List<CalendarEventDto> CalendarEvents { get; set; } = new();
    }
}
