using ClupApi.Models;
using ClupApi.DTOs;
using ClupApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace ClupApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityRepository _repository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;

        public ActivitiesController(IActivityRepository repository, IMapper mapper, IWebHostEnvironment environment)
        {
            _repository = repository;
            _mapper = mapper;
            _environment = environment;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var activities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<List<ActivityResponseDto>>(activities);
            return Ok(dtos);
        }

        /// <summary>
        /// n8n öneri sistemi için: Sadece aktif, silinmemiş ve henüz başlamamış/devam eden etkinlikleri döndürür
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailable()
        {
            var activities = await _repository.GetAllAsync();
            var now = DateTime.UtcNow;
            
            var availableActivities = activities
                .Where(a => a.IsActive && !a.IsDeleted && a.EndDate > now)
                .ToList();
            
            var dtos = _mapper.Map<List<ActivityResponseDto>>(availableActivities);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var activity = await _repository.GetByIdAsync(id);
            if (activity == null) return NotFound();
            var dto = _mapper.Map<ActivityResponseDto>(activity);
            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> Create([FromBody] ActivityCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Auto-set OrganizingClubID from authenticated club (security requirement)
            var currentClubId = GetCurrentClubId();

            var activity = new Activity
            {
                ActivityName = createDto.ActivityName,
                ActivityDescription = createDto.ActivityDescription,
                OrganizingClubID = currentClubId, // Always use authenticated club's ID
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                ParticipantLimit = createDto.ParticipantLimit,
                EvaluationStartDate = createDto.EvaluationStartDate,
                EvaluationEndDate = createDto.EvaluationEndDate,
                ImagePath = createDto.ImagePath,
                CreationDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                NumberOfParticipants = 0
            };

            await _repository.AddAsync(activity);
            return CreatedAtAction(nameof(GetById), new { id = activity.ActivityID }, activity);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] ActivityUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingActivity = await _repository.GetByIdAsync(id);
            if (existingActivity == null) 
                return NotFound();

            // Ownership check - clubs can only update their own activities
            var currentClubId = GetCurrentClubId();
            if (existingActivity.OrganizingClubID != currentClubId)
            {
                return Forbid();
            }

            existingActivity.ActivityName = updateDto.ActivityName;
            existingActivity.ActivityDescription = updateDto.ActivityDescription;
            existingActivity.StartDate = updateDto.StartDate;
            existingActivity.EndDate = updateDto.EndDate;
            existingActivity.ParticipantLimit = updateDto.ParticipantLimit;
            existingActivity.EvaluationStartDate = updateDto.EvaluationStartDate;
            existingActivity.EvaluationEndDate = updateDto.EvaluationEndDate;
            existingActivity.ImagePath = updateDto.ImagePath;

            await _repository.UpdateAsync(existingActivity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingActivity = await _repository.GetByIdAsync(id);
            if (existingActivity == null)
                return NotFound();

            // Ownership check - clubs can only delete their own activities
            var currentClubId = GetCurrentClubId();
            if (existingActivity.OrganizingClubID != currentClubId)
            {
                return Forbid();
            }

            await _repository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("upload-image")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Dosya seçilmedi." });

            // Dosya uzantısı kontrolü
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Sadece resim dosyaları yüklenebilir (jpg, jpeg, png, gif, webp)." });

            // Dosya boyutu kontrolü (5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Dosya boyutu 5MB'dan büyük olamaz." });

            try
            {
                // ActivityImages klasörünü oluştur - ContentRootPath kullan
                var webRootPath = _environment.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                }
                
                var uploadsFolder = Path.Combine(webRootPath, "Images", "ActivityImages");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Benzersiz dosya adı oluştur
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Veritabanına kaydedilecek yol
                var imagePath = $"/Images/ActivityImages/{uniqueFileName}";

                return Ok(new { imagePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Dosya yüklenirken hata oluştu: {ex.Message}" });
            }
        }

        private int GetCurrentClubId()
        {
            // Token'da "userId" claim'i kullanılıyor
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var clubId) ? clubId : 0;
        }
    }
}
