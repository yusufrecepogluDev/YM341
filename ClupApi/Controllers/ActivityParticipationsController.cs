using Microsoft.AspNetCore.Mvc;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;

namespace ClupApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityParticipationsController : BaseController
    {
        private readonly IActivityParticipationRepository _participationRepository;

        public ActivityParticipationsController(IActivityParticipationRepository participationRepository)
        {
            _participationRepository = participationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var participations = await _participationRepository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ActivityParticipation>>.SuccessResponse(participations));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var participation = await _participationRepository.GetByIdAsync(id);
            return HandleResult(participation);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudentId(int studentId)
        {
            var participations = await _participationRepository.GetByStudentIdAsync(studentId);
            return Ok(ApiResponse<IEnumerable<ActivityParticipation>>.SuccessResponse(participations));
        }

        [HttpGet("activity/{activityId}")]
        public async Task<IActionResult> GetByActivityId(int activityId)
        {
            var participations = await _participationRepository.GetByActivityIdAsync(activityId);
            return Ok(ApiResponse<IEnumerable<ActivityParticipation>>.SuccessResponse(participations));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ActivityParticipation participation)
        {
            var created = await _participationRepository.CreateAsync(participation);
            return HandleCreatedResult(created, nameof(GetById), new { id = created.ParticipationID });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActivityParticipation participation)
        {
            participation.ParticipationID = id;
            var updated = await _participationRepository.UpdateAsync(participation);
            return HandleUpdatedResult(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _participationRepository.DeleteAsync(id);
            return result ? HandleDeletedResult() : HandleNotFound();
        }
    }
}
