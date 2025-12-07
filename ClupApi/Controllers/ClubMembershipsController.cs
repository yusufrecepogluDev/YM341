using Microsoft.AspNetCore.Mvc;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;

namespace ClupApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClubMembershipsController : BaseController
    {
        private readonly IClubMembershipRepository _membershipRepository;

        public ClubMembershipsController(IClubMembershipRepository membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var memberships = await _membershipRepository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ClubMembership>>.SuccessResponse(memberships));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var membership = await _membershipRepository.GetByIdAsync(id);
            return HandleResult(membership);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudentId(int studentId)
        {
            var memberships = await _membershipRepository.GetByStudentIdAsync(studentId);
            return Ok(ApiResponse<IEnumerable<ClubMembership>>.SuccessResponse(memberships));
        }

        [HttpGet("club/{clubId}")]
        public async Task<IActionResult> GetByClubId(int clubId)
        {
            var memberships = await _membershipRepository.GetByClubIdAsync(clubId);
            return Ok(ApiResponse<IEnumerable<ClubMembership>>.SuccessResponse(memberships));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClubMembership membership)
        {
            var created = await _membershipRepository.CreateAsync(membership);
            return HandleCreatedResult(created, nameof(GetById), new { id = created.MembershipID });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ClubMembership membership)
        {
            membership.MembershipID = id;
            var updated = await _membershipRepository.UpdateAsync(membership);
            return HandleUpdatedResult(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _membershipRepository.DeleteAsync(id);
            return result ? HandleDeletedResult() : HandleNotFound();
        }
    }
}
