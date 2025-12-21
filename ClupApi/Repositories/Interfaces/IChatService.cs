using ClupApi.DTOs;

namespace ClupApi.Repositories.Interfaces
{
    public interface IChatService
    {

        Task<ChatResponseDto> SendToN8nAsync(string message, string userId, string? sessionId = null, string? contextData = null);

        Task<CalendarContextDto> GetCalendarContextAsync();

        string FormatContextData(CalendarContextDto context);
    }
}
