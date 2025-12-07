using System.ComponentModel.DataAnnotations;

namespace KampusEtkinlik.Data.DTOs
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
    /// Request DTO for sending a chat message to backend
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
    /// Response DTO from backend chat service
    /// </summary>
    public class ChatResponseDto
    {
        [Required(ErrorMessage = "Yanıt zorunludur")]
        public string Response { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public bool Success { get; set; } = true;
        
        public string? ErrorMessage { get; set; }
    }
}
