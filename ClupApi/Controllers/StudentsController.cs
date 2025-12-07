using Microsoft.AspNetCore.Mvc;
using ClupApi.Models;
using ClupApi.Repositories.Interfaces;

namespace ClupApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> Create([FromBody] Student student)
        {
            var created = await _studentRepository.CreateAsync(student);
            return HandleCreatedResult(created, nameof(GetById), new { id = created.StudentID });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Student student)
        {
            student.StudentID = id;
            var updated = await _studentRepository.UpdateAsync(student);
            return HandleUpdatedResult(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _studentRepository.DeleteAsync(id);
            return result ? HandleDeletedResult() : HandleNotFound();
        }
    }
}
