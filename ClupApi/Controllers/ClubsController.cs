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
    public class ClubsController : BaseController
    {
        private readonly IClubRepository _clubRepository;
        private readonly IMapper _mapper;

        public ClubsController(IClubRepository clubRepository, IMapper mapper)
        {
            _clubRepository = clubRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous] // Public club listing
        public async Task<IActionResult> GetAll()
        {
            var clubs = await _clubRepository.GetAllAsync();
            var clubDtos = _mapper.Map<IEnumerable<ClubResponseDto>>(clubs);
            return Ok(ApiResponse<IEnumerable<ClubResponseDto>>.SuccessResponse(clubDtos));
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // Public club details
        public async Task<IActionResult> GetById(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            if (club == null)
                return HandleNotFound();
            var clubDto = _mapper.Map<ClubResponseDto>(club);
            return Ok(ApiResponse<ClubResponseDto>.SuccessResponse(clubDto));
        }

        [HttpPost]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> Create([FromBody] ClubCreateDto clubDto)
        {
            var club = _mapper.Map<Club>(clubDto);
            var created = await _clubRepository.CreateAsync(club);
            var createdDto = _mapper.Map<ClubResponseDto>(created);
            return Ok(ApiResponse<ClubResponseDto>.SuccessResponse(createdDto));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] ClubUpdateDto clubDto)
        {
            // Ownership check - clubs can only update their own data
            var currentClubId = GetCurrentClubId();
            if (currentClubId != id)
            {
                return Forbid();
            }

            var existingClub = await _clubRepository.GetByIdAsync(id);
            if (existingClub == null)
                return HandleNotFound();

            _mapper.Map(clubDto, existingClub);
            var updated = await _clubRepository.UpdateAsync(existingClub);
            var updatedDto = _mapper.Map<ClubResponseDto>(updated);
            return Ok(ApiResponse<ClubResponseDto>.SuccessResponse(updatedDto));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ClubOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            // Ownership check - clubs can only delete their own data
            var currentClubId = GetCurrentClubId();
            if (currentClubId != id)
            {
                return Forbid();
            }

            var result = await _clubRepository.DeleteAsync(id);
            return result ? HandleDeletedResult() : HandleNotFound();
        }

        private int GetCurrentClubId()
        {
            // Token'da "userId" claim'i kullanılıyor
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var clubId) ? clubId : 0;
        }
    }
}
