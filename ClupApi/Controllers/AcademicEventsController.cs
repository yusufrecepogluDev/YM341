using ClupApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClupApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcademicEventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AcademicEventsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm akademik etkinlikleri getirir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.AcademicEvents.ToListAsync();
            return Ok(ApiResponse<IEnumerable<AcademicEvents>>.SuccessResponse(events));
        }

        /// <summary>
        /// ID'ye göre akademik etkinlik getirir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var academicEvent = await _context.AcademicEvents.FindAsync(id);
            if (academicEvent == null)
                return NotFound(ApiResponse.ErrorResponse("Akademik etkinlik bulunamadı", new[] { "ID bulunamadı" }));
            
            return Ok(ApiResponse<AcademicEvents>.SuccessResponse(academicEvent));
        }

        /// <summary>
        /// Kategoriye göre akademik etkinlikleri getirir
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var events = await _context.AcademicEvents
                .Where(e => e.Category == category)
                .ToListAsync();
            
            return Ok(ApiResponse<IEnumerable<AcademicEvents>>.SuccessResponse(events));
        }

        /// <summary>
        /// Gelecek akademik etkinlikleri getirir
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming()
        {
            var today = DateTime.Today;
            var events = await _context.AcademicEvents
                .Where(e => e.StartDate >= today)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
            
            return Ok(ApiResponse<IEnumerable<AcademicEvents>>.SuccessResponse(events));
        }
    }
}