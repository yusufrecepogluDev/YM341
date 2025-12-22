using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using ClupApi.DTOs;
using AutoMapper;
using System.Security.Claims;

namespace ClupApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClubMembershipsController : BaseController
    {
        private readonly IClubMembershipRepository _membershipRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public ClubMembershipsController(IClubMembershipRepository membershipRepository, IStudentRepository studentRepository, IMapper mapper)
        {
            _membershipRepository = membershipRepository;
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        /// Öğrencinin tüm üyeliklerini getirir (n8n öneri sistemi için - public)
 
        [HttpGet("student/{studentId}/public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByStudentIdPublic(int studentId)
        {
            var memberships = await _membershipRepository.GetByStudentIdAsync(studentId);
            // Sadece onaylanmış üyelikleri döndür
            var approvedMemberships = memberships.Where(m => m.IsApproved == true);
            var membershipDtos = _mapper.Map<IEnumerable<ClubMembershipResponseDto>>(approvedMemberships);
            return Ok(ApiResponse<IEnumerable<ClubMembershipResponseDto>>.SuccessResponse(membershipDtos));
        }

 
        /// Öğrencinin tüm üyeliklerini getirir
 
        [HttpGet("student/{studentId}")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> GetByStudentId(int studentId)
        {
            // Debug log
            var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            Console.WriteLine($"GetByStudentId - Claims: {string.Join(", ", allClaims)}");
            
            // Öğrenci sadece kendi üyeliklerini görebilir
            var currentStudentId = GetCurrentStudentId();
            Console.WriteLine($"GetByStudentId - currentStudentId: {currentStudentId}, requested studentId: {studentId}");
            
            if (currentStudentId != studentId)
                return Forbid();

            var memberships = await _membershipRepository.GetByStudentIdAsync(studentId);
            Console.WriteLine($"GetByStudentId - Found {memberships.Count()} memberships");
            
            var membershipDtos = _mapper.Map<IEnumerable<ClubMembershipResponseDto>>(memberships);
            return Ok(ApiResponse<IEnumerable<ClubMembershipResponseDto>>.SuccessResponse(membershipDtos));
        }

 
        /// Kulübün onaylanmış üyelerini getirir
 
        [HttpGet("club/{clubId}")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> GetByClubId(int clubId)
        {
            // Kulüp sadece kendi üyelerini görebilir
            var currentClubId = GetCurrentClubId();
            if (currentClubId != clubId)
                return Forbid();

            var memberships = await _membershipRepository.GetByClubIdAsync(clubId);
            // Sadece onaylanmış üyeleri getir
            var approvedMemberships = memberships.Where(m => m.IsApproved == true);
            var membershipDtos = _mapper.Map<IEnumerable<ClubMembershipResponseDto>>(approvedMemberships);
            return Ok(ApiResponse<IEnumerable<ClubMembershipResponseDto>>.SuccessResponse(membershipDtos));
        }

 
        /// Kulübün bekleyen başvurularını getirir
 
        [HttpGet("club/{clubId}/pending")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> GetPendingByClubId(int clubId)
        {
            // Kulüp sadece kendi bekleyen başvurularını görebilir
            var currentClubId = GetCurrentClubId();
            if (currentClubId != clubId)
                return Forbid();

            var memberships = await _membershipRepository.GetByClubIdAsync(clubId);
            // Sadece bekleyen başvuruları getir (IsApproved == null)
            var pendingMemberships = memberships.Where(m => m.IsApproved == null);
            var membershipDtos = _mapper.Map<IEnumerable<ClubMembershipResponseDto>>(pendingMemberships);
            return Ok(ApiResponse<IEnumerable<ClubMembershipResponseDto>>.SuccessResponse(membershipDtos));
        }

 
        /// Kulübe üyelik başvurusu yapar
 
        [HttpPost]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> Apply([FromBody] ClubMembershipApplyDto applyDto)
        {
            var currentStudentId = GetCurrentStudentId();
            
            // Debug: StudentID'yi kontrol et
            if (currentStudentId <= 0)
                return BadRequest(ApiResponse<object>.ErrorResponse("Geçersiz öğrenci ID'si.", new[] { "Invalid student ID from token" }));

            // Öğrencinin varlığını kontrol et
            var student = await _studentRepository.GetByIdAsync(currentStudentId);
            if (student == null)
                return BadRequest(ApiResponse<object>.ErrorResponse($"Öğrenci bulunamadı. ID: {currentStudentId}", new[] { "Student not found in database" }));

            // Zaten üyelik veya bekleyen başvuru var mı kontrol et
            var existingMemberships = await _membershipRepository.GetByStudentIdAsync(currentStudentId);
            if (existingMemberships.Any(m => m.ClubID == applyDto.ClubID))
                return BadRequest(ApiResponse<object>.ErrorResponse("Bu kulübe zaten başvurunuz veya üyeliğiniz bulunmaktadır.", new[] { "Duplicate membership application" }));

            var membership = new ClubMembership
            {
                StudentID = currentStudentId,
                ClubID = applyDto.ClubID,
                JoinDate = DateTime.UtcNow,
                IsApproved = null // Beklemede
            };

            var created = await _membershipRepository.CreateAsync(membership);
            var createdDto = _mapper.Map<ClubMembershipResponseDto>(created);
            return Ok(ApiResponse<ClubMembershipResponseDto>.SuccessResponse(createdDto, "Üyelik başvurunuz alındı."));
        }

 
        /// Başvuruyu onaylar
 
        [HttpPut("{id}/approve")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> Approve(int id)
        {
            var membership = await _membershipRepository.GetByIdAsync(id);
            if (membership == null)
                return HandleNotFound();

            // Kulüp sadece kendi başvurularını onaylayabilir
            var currentClubId = GetCurrentClubId();
            if (membership.ClubID != currentClubId)
                return Forbid();

            // Zaten onaylanmış mı kontrol et
            if (membership.IsApproved == true)
                return BadRequest(ApiResponse<object>.ErrorResponse("Bu başvuru zaten onaylanmış.", new[] { "Application already approved" }));

            membership.IsApproved = true;
            membership.JoinDate = DateTime.UtcNow;
            var updated = await _membershipRepository.UpdateAsync(membership);
            var updatedDto = _mapper.Map<ClubMembershipResponseDto>(updated);
            return Ok(ApiResponse<ClubMembershipResponseDto>.SuccessResponse(updatedDto, "Başvuru onaylandı."));
        }

 
        /// Başvuruyu reddeder
 
        [HttpPut("{id}/reject")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> Reject(int id)
        {
            var membership = await _membershipRepository.GetByIdAsync(id);
            if (membership == null)
                return HandleNotFound();

            // Kulüp sadece kendi başvurularını reddedebilir
            var currentClubId = GetCurrentClubId();
            if (membership.ClubID != currentClubId)
                return Forbid();

            // Zaten onaylanmış üyelik reddedilemez
            if (membership.IsApproved == true)
                return BadRequest(ApiResponse<object>.ErrorResponse("Onaylanmış üyelik reddedilemez. Üyeyi çıkarmak için silme işlemi kullanın.", new[] { "Cannot reject approved membership" }));

            // Başvuruyu sil
            await _membershipRepository.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Başvuru reddedildi."));
        }

 
        /// Üyelikten ayrılır (öğrenci) veya üyeyi çıkarır (kulüp)
 
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Leave(int id)
        {
            var membership = await _membershipRepository.GetByIdAsync(id);
            if (membership == null)
                return HandleNotFound();

            var currentStudentId = GetCurrentStudentId();
            var currentClubId = GetCurrentClubId();

            // Öğrenci kendi üyeliğinden ayrılabilir veya kulüp kendi üyesini çıkarabilir
            bool isOwner = (currentStudentId > 0 && membership.StudentID == currentStudentId) ||
                          (currentClubId > 0 && membership.ClubID == currentClubId);

            if (!isOwner)
                return Forbid();

            await _membershipRepository.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Üyelik sonlandırıldı."));
        }

        private int GetCurrentStudentId()
        {
            // Token'da "userId" claim'i kullanılıyor
            var userIdClaim = User.FindFirst("userId")?.Value;
            var userType = User.FindFirst("userType")?.Value;
            
            // API'de userType küçük harfle "student" olarak kaydediliyor
            if (string.Equals(userType, "student", StringComparison.OrdinalIgnoreCase) && int.TryParse(userIdClaim, out var studentId))
                return studentId;
            return 0;
        }

        private int GetCurrentClubId()
        {
            // Token'da "userId" claim'i kullanılıyor
            var userIdClaim = User.FindFirst("userId")?.Value;
            var userType = User.FindFirst("userType")?.Value;
            
            // API'de userType küçük harfle "club" olarak kaydediliyor
            if (string.Equals(userType, "club", StringComparison.OrdinalIgnoreCase) && int.TryParse(userIdClaim, out var clubId))
                return clubId;
            return 0;
        }
    }
}
