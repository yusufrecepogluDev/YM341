using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using ClupApi.DTOs;
using AutoMapper;

namespace ClupApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityParticipationsController : BaseController
    {
        private readonly IActivityParticipationRepository _participationRepository;
        private readonly ClupApi.Repositories.IActivityRepository _activityRepository;
        private readonly IMapper _mapper;

        public ActivityParticipationsController(
            IActivityParticipationRepository participationRepository,
            ClupApi.Repositories.IActivityRepository activityRepository,
            IMapper mapper)
        {
            _participationRepository = participationRepository;
            _activityRepository = activityRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var participations = await _participationRepository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<ActivityParticipationResponseDto>>(participations);
            return Ok(ApiResponse<IEnumerable<ActivityParticipationResponseDto>>.SuccessResponse(dtos));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var participation = await _participationRepository.GetByIdAsync(id);
            if (participation == null)
                return HandleNotFound();
            var dto = _mapper.Map<ActivityParticipationResponseDto>(participation);
            return Ok(ApiResponse<ActivityParticipationResponseDto>.SuccessResponse(dto));
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudentId(int studentId)
        {
            var participations = await _participationRepository.GetByStudentIdAsync(studentId);
            var dtos = _mapper.Map<IEnumerable<ActivityParticipationResponseDto>>(participations);
            return Ok(ApiResponse<IEnumerable<ActivityParticipationResponseDto>>.SuccessResponse(dtos));
        }

        [HttpGet("activity/{activityId}")]
        public async Task<IActionResult> GetByActivityId(int activityId)
        {
            var participations = await _participationRepository.GetByActivityIdAsync(activityId);
            var dtos = _mapper.Map<IEnumerable<ActivityParticipationResponseDto>>(participations);
            return Ok(ApiResponse<IEnumerable<ActivityParticipationResponseDto>>.SuccessResponse(dtos));
        }

        /// <summary>
        /// Öğrencinin belirli bir etkinliğe katılım durumunu kontrol eder
        /// </summary>
        [HttpGet("check/{activityId}/{studentId}")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> CheckParticipation(int activityId, int studentId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId != studentId)
                return Forbid();

            var participation = await _participationRepository.GetParticipationAsync(studentId, activityId);
            var isParticipating = participation != null;
            
            // DTO kullanarak döngüsel referansı önle
            ActivityParticipationResponseDto? participationDto = null;
            if (participation != null)
            {
                participationDto = _mapper.Map<ActivityParticipationResponseDto>(participation);
            }
            
            return Ok(ApiResponse<object>.SuccessResponse(new { isParticipating, participation = participationDto }));
        }


        /// <summary>
        /// Etkinliğe katılım (Kayıt ol)
        /// </summary>
        [HttpPost("join/{activityId}")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> JoinActivity(int activityId)
        {
            var studentId = GetCurrentUserId();
            if (studentId == 0)
                return Unauthorized(ApiResponse.ErrorResponse("Oturum açmanız gerekiyor", Array.Empty<string>()));

            // Etkinlik kontrolü
            var activity = await _activityRepository.GetByIdAsync(activityId);
            if (activity == null)
                return NotFound(ApiResponse.ErrorResponse("Etkinlik bulunamadı", Array.Empty<string>()));

            if (!activity.IsActive || activity.IsDeleted)
                return BadRequest(ApiResponse.ErrorResponse("Bu etkinlik aktif değil", Array.Empty<string>()));

            // Zaten katılmış mı kontrolü
            var existingParticipation = await _participationRepository.GetParticipationAsync(studentId, activityId);
            if (existingParticipation != null)
                return BadRequest(ApiResponse.ErrorResponse("Bu etkinliğe zaten kayıtlısınız", Array.Empty<string>()));

            // Kontenjan kontrolü
            if (activity.ParticipantLimit.HasValue)
            {
                var currentCount = await _participationRepository.GetParticipantCountAsync(activityId);
                if (currentCount >= activity.ParticipantLimit.Value)
                    return BadRequest(ApiResponse.ErrorResponse("Etkinlik kontenjanı dolmuştur", Array.Empty<string>()));
            }

            // Etkinlik bitmiş mi kontrolü (başlamış olsa bile kayıt olunabilir)
            if (activity.EndDate < DateTime.UtcNow)
                return BadRequest(ApiResponse.ErrorResponse("Bu etkinlik sona ermiş", Array.Empty<string>()));

            // Katılım oluştur
            var participation = new ActivityParticipation
            {
                ActivityID = activityId,
                StudentID = studentId,
                JoinDate = DateTime.UtcNow
            };

            var created = await _participationRepository.CreateAsync(participation);
            
            // Katılımcı sayısını güncelle
            activity.NumberOfParticipants = (activity.NumberOfParticipants ?? 0) + 1;
            await _activityRepository.UpdateAsync(activity);

            var createdDto = _mapper.Map<ActivityParticipationResponseDto>(created);
            return Ok(ApiResponse<ActivityParticipationResponseDto>.SuccessResponse(createdDto, "Etkinliğe başarıyla kayıt oldunuz"));
        }

        /// <summary>
        /// Etkinlikten ayrılma (Kayıt iptal)
        /// </summary>
        [HttpDelete("leave/{activityId}")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> LeaveActivity(int activityId)
        {
            var studentId = GetCurrentUserId();
            if (studentId == 0)
                return Unauthorized(ApiResponse.ErrorResponse("Oturum açmanız gerekiyor", Array.Empty<string>()));

            // Etkinlik kontrolü
            var activity = await _activityRepository.GetByIdAsync(activityId);
            if (activity == null)
                return NotFound(ApiResponse.ErrorResponse("Etkinlik bulunamadı", Array.Empty<string>()));

            // Katılım kontrolü
            var participation = await _participationRepository.GetParticipationAsync(studentId, activityId);
            if (participation == null)
                return BadRequest(ApiResponse.ErrorResponse("Bu etkinliğe kayıtlı değilsiniz", Array.Empty<string>()));

            // Etkinlik bitmiş mi kontrolü (bitmiş etkinlikten ayrılamaz)
            if (activity.EndDate < DateTime.UtcNow)
                return BadRequest(ApiResponse.ErrorResponse("Bitmiş etkinlikten ayrılamazsınız", Array.Empty<string>()));

            // Katılımı sil
            await _participationRepository.DeleteAsync(participation.ParticipationID);
            
            // Katılımcı sayısını güncelle
            if (activity.NumberOfParticipants > 0)
            {
                activity.NumberOfParticipants = activity.NumberOfParticipants - 1;
                await _activityRepository.UpdateAsync(activity);
            }

            return Ok(ApiResponse.SuccessResponse("Etkinlik kaydınız iptal edildi"));
        }

        [HttpPost]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> Create([FromBody] ActivityParticipationCreateDto dto)
        {
            var participation = new ActivityParticipation
            {
                ActivityID = dto.ActivityID,
                StudentID = dto.StudentID,
                JoinDate = dto.JoinDate ?? DateTime.UtcNow,
                Rating = dto.Rating
            };
            
            var created = await _participationRepository.CreateAsync(participation);
            var createdDto = _mapper.Map<ActivityParticipationResponseDto>(created);
            return Ok(ApiResponse<ActivityParticipationResponseDto>.SuccessResponse(createdDto, "Katılım kaydı oluşturuldu"));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] ActivityParticipationUpdateDto dto)
        {
            var existing = await _participationRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse.ErrorResponse("Katılım kaydı bulunamadı", Array.Empty<string>()));

            var currentUserId = GetCurrentUserId();
            if (existing.StudentID != currentUserId)
                return Forbid();

            existing.Rating = dto.Rating;
            var updated = await _participationRepository.UpdateAsync(existing);
            var updatedDto = _mapper.Map<ActivityParticipationResponseDto>(updated);
            return Ok(ApiResponse<ActivityParticipationResponseDto>.SuccessResponse(updatedDto, "Katılım kaydı güncellendi"));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _participationRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (existing.StudentID != currentUserId)
                return Forbid();

            var result = await _participationRepository.DeleteAsync(id);
            return result ? HandleDeletedResult() : HandleNotFound();
        }

        /// <summary>
        /// Etkinliğe puan verir (1-5 yıldız)
        /// </summary>
        [HttpPut("rate/{activityId}")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> RateActivity(int activityId, [FromBody] RatingDto ratingDto)
        {
            var studentId = GetCurrentUserId();
            if (studentId == 0)
                return Unauthorized(ApiResponse.ErrorResponse("Oturum açmanız gerekiyor", Array.Empty<string>()));

            // Katılım kontrolü
            var participation = await _participationRepository.GetParticipationAsync(studentId, activityId);
            if (participation == null)
                return BadRequest(ApiResponse.ErrorResponse("Bu etkinliğe kayıtlı değilsiniz", Array.Empty<string>()));

            // Etkinlik kontrolü - değerlendirme süresi
            var activity = await _activityRepository.GetByIdAsync(activityId);
            if (activity == null)
                return NotFound(ApiResponse.ErrorResponse("Etkinlik bulunamadı", Array.Empty<string>()));

            // Değerlendirme tarihi kontrolü: Etkinlik başlangıcından bitiş + 1 gün sonrasına kadar
            var now = DateTime.UtcNow;
            if (now < activity.StartDate)
                return BadRequest(ApiResponse.ErrorResponse("Etkinlik henüz başlamadı, değerlendirme yapamazsınız", Array.Empty<string>()));
            
            if (now > activity.EndDate.AddDays(1))
                return BadRequest(ApiResponse.ErrorResponse("Değerlendirme süresi dolmuştur", Array.Empty<string>()));

            // Rating güncelle
            participation.Rating = ratingDto.Rating;
            await _participationRepository.UpdateAsync(participation);

            return Ok(ApiResponse.SuccessResponse("Değerlendirmeniz kaydedildi"));
        }

        /// <summary>
        /// Etkinliğin ortalama puanını getirir
        /// </summary>
        [HttpGet("rating/{activityId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivityRating(int activityId)
        {
            var participations = await _participationRepository.GetByActivityIdAsync(activityId);
            var ratings = participations.Where(p => p.Rating.HasValue).Select(p => p.Rating!.Value).ToList();
            
            var averageRating = ratings.Any() ? Math.Round(ratings.Average(), 1) : 0;
            var ratingCount = ratings.Count;

            return Ok(ApiResponse<object>.SuccessResponse(new { averageRating, ratingCount }));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
