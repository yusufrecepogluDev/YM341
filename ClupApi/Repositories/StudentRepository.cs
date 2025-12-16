using ClupApi.Models;
using ClupApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClupApi.Repositories
{
    public class StudentRepository : BaseRepository<Student>, IStudentRepository
    {
        public StudentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Student?> GetByStudentNumberAsync(long studentNumber)
        {
            return await _dbSet
                .Where(s => s.StudentNumber == studentNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<Student?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Where(s => s.StudentMail == email)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
        {
            return await _dbSet
                .Where(s => s.IsActive)
                .ToListAsync();
        }
    }
}