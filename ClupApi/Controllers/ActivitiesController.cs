using ClupApi.Models;
using ClupApi.DTOs;
using ClupApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ClupApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityRepository _repository;

        public ActivitiesController(IActivityRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var activities = await _repository.GetAllAsync();
            return Ok(activities);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var activity = await _repository.GetByIdAsync(id);
            if (activity == null) return NotFound();
            return Ok(activity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ActivityCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var activity = new Activity
            {
                ActivityName = createDto.ActivityName,
                ActivityDescription = createDto.ActivityDescription,
                OrganizingClubID = createDto.OrganizingClubID,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                ParticipantLimit = createDto.ParticipantLimit,
                EvaluationStartDate = createDto.EvaluationStartDate,
                EvaluationEndDate = createDto.EvaluationEndDate,
                CreationDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                NumberOfParticipants = 0
            };

            await _repository.AddAsync(activity);
            return CreatedAtAction(nameof(GetById), new { id = activity.ActivityID }, activity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ActivityUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingActivity = await _repository.GetByIdAsync(id);
            if (existingActivity == null) 
                return NotFound();

            existingActivity.ActivityName = updateDto.ActivityName;
            existingActivity.ActivityDescription = updateDto.ActivityDescription;
            existingActivity.StartDate = updateDto.StartDate;
            existingActivity.EndDate = updateDto.EndDate;
            existingActivity.ParticipantLimit = updateDto.ParticipantLimit;
            existingActivity.EvaluationStartDate = updateDto.EvaluationStartDate;
            existingActivity.EvaluationEndDate = updateDto.EvaluationEndDate;

            await _repository.UpdateAsync(existingActivity);
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
