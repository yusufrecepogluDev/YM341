using System.ComponentModel.DataAnnotations;

namespace KampusEtkinlik.Data.DTOs
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
}
