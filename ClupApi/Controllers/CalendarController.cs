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

        /// <summary>
        /// Belirli bir tarih aralığındaki tüm etkinlikleri getirir
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi</param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns>Etkinlik listesi</returns>
        [HttpGet("events")]
        public async Task<IActionResult> GetEvents(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            // Validasyon: startDate < endDate
            if (startDate > endDate)
            {
                return HandleBadRequest("Başlangıç tarihi bitiş tarihinden sonra olamaz");
            }

            // Validasyon: Maksimum 3 aylık aralık (performans için)
            var maxRange = startDate.AddMonths(3);
            if (endDate > maxRange)
            {
                return HandleBadRequest("Maksimum 3 aylık tarih aralığı sorgulanabilir");
            }

            var events = await _calendarRepository.GetEventsByDateRangeAsync(startDate, endDate);
            return HandleResult(events);
        }

        /// <summary>
        /// Belirli bir günün tüm etkinliklerini getirir
        /// </summary>
        /// <param name="date">Tarih</param>
        /// <returns>Günlük etkinlik listesi</returns>
        [HttpGet("events/daily")]
        public async Task<IActionResult> GetDailyEvents(
            [FromQuery] DateTime date)
        {
            var events = await _calendarRepository.GetEventsByDateAsync(date);
            return HandleResult(events);
        }

        /// <summary>
        /// Tüm etkinlik kategorilerini getirir
        /// </summary>
        /// <returns>Kategori listesi</returns>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _calendarRepository.GetCategoriesAsync();
            return HandleResult(categories);
        }
    }
}
