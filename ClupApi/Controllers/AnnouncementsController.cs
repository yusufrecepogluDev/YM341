using ClupApi.Models;
using ClupApi.DTOs;
using ClupApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
        [AllowAnonymous] // Public announcement listing
        public async Task<IActionResult> GetAll()
        {
            var announcements = await _repository.GetAllAsync();
            return Ok(announcements);
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // Public announcement details
        public async Task<IActionResult> GetById(int id)
        {
            var announcement = await _repository.GetByIdAsync(id);
            if (announcement == null) return NotFound();
            return Ok(announcement);
        }

        [HttpPost]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> Create([FromBody] AnnouncementCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Auto-set ClubID from authenticated club (security requirement)
            var currentClubId = GetCurrentClubId();

            var announcement = new Announcement
            {
                AnnouncementTitle = createDto.AnnouncementTitle,
                AnnouncementContent = createDto.AnnouncementContent,
                ClubID = currentClubId, // Always use authenticated club's ID
                StartDate = createDto.StartDate,
                CreationDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _repository.AddAsync(announcement);
            return CreatedAtAction(nameof(GetById), new { id = announcement.AnnouncementID }, announcement);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] AnnouncementUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingAnnouncement = await _repository.GetByIdAsync(id);
            if (existingAnnouncement == null) 
                return NotFound();

            // Ownership check - clubs can only update their own announcements
            var currentClubId = GetCurrentClubId();
            if (existingAnnouncement.ClubID != currentClubId)
            {
                return Forbid();
            }

            existingAnnouncement.AnnouncementTitle = updateDto.AnnouncementTitle;
            existingAnnouncement.AnnouncementContent = updateDto.AnnouncementContent;
            existingAnnouncement.StartDate = updateDto.StartDate;

            await _repository.UpdateAsync(existingAnnouncement);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingAnnouncement = await _repository.GetByIdAsync(id);
            if (existingAnnouncement == null)
                return NotFound();

            // Ownership check - clubs can only delete their own announcements
            var currentClubId = GetCurrentClubId();
            if (existingAnnouncement.ClubID != currentClubId)
            {
                return Forbid();
            }

            await _repository.DeleteAsync(id);
            return NoContent();
        }

        private int GetCurrentClubId()
        {
            // Token'da "userId" claim'i kullanılıyor
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var clubId) ? clubId : 0;
        }
    }
}
