using ClupApi.DTOs;
using ClupApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClupApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : BaseController
    {
        private readonly ICalendarRepository _calendarRepository;

        public CalendarController(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return HandleBadRequest("Başlangıç tarihi bitiş tarihinden sonra olamaz");
            }

            var maxRange = startDate.AddMonths(3);
            if (endDate > maxRange)
            {
                return HandleBadRequest("Maksimum 3 aylık tarih aralığı sorgulanabilir");
            }

            var events = await _calendarRepository.GetEventsByDateRangeAsync(startDate, endDate);
            return HandleResult(events);
        }

        [HttpGet("events/daily")]
        public async Task<IActionResult> GetDailyEvents(
            [FromQuery] DateTime date)
        {
            var events = await _calendarRepository.GetEventsByDateAsync(date);
            return HandleResult(events);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _calendarRepository.GetCategoriesAsync();
            return HandleResult(categories);
        }
    }
}
