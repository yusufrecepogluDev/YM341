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
        /// <param name="contextData">Optional calendar context data (sent every 14 messages)</param>
        /// <returns>ChatResponseDto containing the bot's response</returns>
        Task<ChatResponseDto> SendToN8nAsync(string message, string userId, string? sessionId = null, string? contextData = null);

        /// <summary>
        /// Gets calendar context data containing upcoming activities and active announcements
        /// </summary>
        /// <returns>CalendarContextDto with upcoming activities (max 10) and active announcements (max 5)</returns>
        Task<CalendarContextDto> GetCalendarContextAsync();

        /// <summary>
        /// Formats calendar context data into a readable text format for the chatbot
        /// </summary>
        /// <param name="context">The calendar context data to format</param>
        /// <returns>Formatted string containing activities and announcements</returns>
        string FormatContextData(CalendarContextDto context);
    }
}
