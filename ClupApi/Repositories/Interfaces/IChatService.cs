using ClupApi.DTOs;

namespace ClupApi.Repositories.Interfaces
{
    /// <summary>
    /// Service interface for managing chat communication with N8n webhook
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Sends a chat message to N8n webhook and returns the bot response
        /// </summary>
        /// <param name="message">The user's message</param>
        /// <param name="userId">The authenticated user's ID</param>
        /// <param name="sessionId">Optional session ID for conversation context</param>
        /// <returns>ChatResponseDto containing the bot's response</returns>
        Task<ChatResponseDto> SendToN8nAsync(string message, string userId, string? sessionId = null);
    }
}
