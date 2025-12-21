using System.ComponentModel.DataAnnotations;

namespace ClupApi.DTOs
{
    public class ChatMessageDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required(ErrorMessage = "Mesaj içeriği zorunludur")]
        public string Content { get; set; } = string.Empty;
        
        public bool IsBot { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public MessageStatus Status { get; set; } = MessageStatus.Sent;
    }

    public enum MessageStatus
    {
        Sent,
        Delivered,
        Error
    }

    public class ChatRequestDto
    {
        [Required(ErrorMessage = "Mesaj zorunludur")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Mesaj 1 ile 500 karakter arasında olmalıdır")]
        public string Message { get; set; } = string.Empty;
        
        public string? SessionId { get; set; }
    }

    public class ChatResponseDto
    {
        [Required(ErrorMessage = "Yanıt zorunludur")]
        public string Response { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public bool Success { get; set; } = true;
        
        public string? ErrorMessage { get; set; }
    }

    public class N8nWebhookRequest
    {
        [Required(ErrorMessage = "Mesaj zorunludur")]
        public string ChatInput { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Kullanıcı ID'si zorunludur")]
        public string UserId { get; set; } = string.Empty;
        
        public string? SessionId { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string? ContextData { get; set; }
    }

    public class N8nWebhookResponse
    {
        [Required(ErrorMessage = "Yanıt zorunludur")]
        public string Response { get; set; } = string.Empty;
        
        public string? SessionId { get; set; }
        
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class CalendarContextDto
    {
        public List<CalendarEventDto> CalendarEvents { get; set; } = new();
    }
}
