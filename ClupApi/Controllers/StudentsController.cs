using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using System.Security.Claims;

namespace ClupApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "StudentOrClub")]
    public class StudentsController : BaseController
    {
        private readonly IStudentRepository _studentRepository;

        public StudentsController(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await _studentRepository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<Student>>.SuccessResponse(students));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            return HandleResult(student);
        }

        [HttpPost]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> Create([FromBody] Student student)
        {
            var created = await _studentRepository.CreateAsync(student);
            return HandleCreatedResult(created, nameof(GetById), new { id = created.StudentID });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] Student student)
        {
            // Ownership check - students can only update their own data
            var currentUserId = GetCurrentUserId();
            if (currentUserId != id)
            {
                return Forbid();
            }

            student.StudentID = id;
            var updated = await _studentRepository.UpdateAsync(student);
            return HandleUpdatedResult(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            // Ownership check - students can only delete their own data
            var currentUserId = GetCurrentUserId();
            if (currentUserId != id)
            {
                return Forbid();
            }

            var result = await _studentRepository.DeleteAsync(id);
            return result ? HandleDeletedResult() : HandleNotFound();
        }

        private int GetCurrentUserId()
        {
            // Token'da "userId" claim'i kullanılıyor
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
