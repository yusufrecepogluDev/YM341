using ClupApi.DTOs;

namespace ClupApi.Repositories.Interfaces
{
    public interface ICalendarRepository : IBaseRepository<CalendarEventDto>
    {
        Task<List<CalendarEventDto>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<CalendarEventDto>> GetEventsByDateAsync(DateTime date);
        Task<List<CategoryDto>> GetCategoriesAsync();
    }
}
