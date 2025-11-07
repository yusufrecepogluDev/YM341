using ClupApi.Models;
using ClupApi.DTOs;
using ClupApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ClupApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementRepository _repository;

        public AnnouncementsController(IAnnouncementRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var announcements = await _repository.GetAllAsync();
            return Ok(announcements);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var announcement = await _repository.GetByIdAsync(id);
            if (announcement == null) return NotFound();
            return Ok(announcement);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AnnouncementCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var announcement = new Announcement
            {
                AnnouncementTitle = createDto.AnnouncementTitle,
                AnnouncementContent = createDto.AnnouncementContent,
                ClubID = createDto.ClubID,
                StartDate = createDto.StartDate,
                CreationDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _repository.AddAsync(announcement);
            return CreatedAtAction(nameof(GetById), new { id = announcement.AnnouncementID }, announcement);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AnnouncementUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingAnnouncement = await _repository.GetByIdAsync(id);
            if (existingAnnouncement == null) 
                return NotFound();

            existingAnnouncement.AnnouncementTitle = updateDto.AnnouncementTitle;
            existingAnnouncement.AnnouncementContent = updateDto.AnnouncementContent;
            existingAnnouncement.StartDate = updateDto.StartDate;

            await _repository.UpdateAsync(existingAnnouncement);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
