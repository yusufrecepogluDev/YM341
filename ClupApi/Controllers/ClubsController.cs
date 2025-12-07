using Microsoft.AspNetCore.Mvc;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;

namespace ClupApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClubsController : BaseController
    {
        private readonly IClubRepository _clubRepository;

        public ClubsController(IClubRepository clubRepository)
        {
            _clubRepository = clubRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clubs = await _clubRepository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<Club>>.SuccessResponse(clubs));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            return HandleResult(club);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Club club)
        {
            var created = await _clubRepository.CreateAsync(club);
            return HandleCreatedResult(created, nameof(GetById), new { id = created.ClubID });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Club club)
        {
            club.ClubID = id;
            var updated = await _clubRepository.UpdateAsync(club);
            return HandleUpdatedResult(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _clubRepository.DeleteAsync(id);
            return result ? HandleDeletedResult() : HandleNotFound();
        }
    }
}
