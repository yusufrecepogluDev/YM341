using ClupApi.DTOs;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClupApi.Repositories
{
    public class CalendarRepository : BaseRepository<CalendarEventDto>, ICalendarRepository
    {
        public CalendarRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<CalendarEventDto>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement
            return new List<CalendarEventDto>();
        }

        public async Task<List<CalendarEventDto>> GetEventsByDateAsync(DateTime date)
        {
            // TODO: Implement
            return new List<CalendarEventDto>();
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            // TODO: Implement
            return new List<CategoryDto>();
        }
    }
}
