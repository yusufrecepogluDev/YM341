using ClupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClupApi.Repositories
{
    public interface IAuthenticationRepository
    {
        Task<Student?> GetStudentByNumberAsync(long studentNumber);
        Task<Club?> GetClubByNumberAsync(long clubNumber);
        Task<Student?> GetStudentByPasswordAsync(string studentPassword);
        Task<Club?> GetClubByPasswordAsync(string clubPassword);
        Task<Student?> GetStudentByMailAsync(string studentMail);
        Task<bool> AddStudentAsync(Student student);
        Task<bool> AddClubAsync(Club club);
    }

    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly AppDbContext _context;

        public AuthenticationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Student?> GetStudentByNumberAsync(long studentNumber)
        {
            return await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
        }

        public async Task<Club?> GetClubByNumberAsync(long clubNumber)
        {
            return await _context.Clubs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClubNumber == clubNumber);
        }
        public async Task<Student?> GetStudentByPasswordAsync(string studentPassword)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.StudentPassword == studentPassword);
        }
        public async Task<Club?> GetClubByPasswordAsync(string clubPassword)
        {
            return await _context.Clubs
                .FirstOrDefaultAsync(c => c.ClubPassword == clubPassword);
        }

        public async Task<Student?> GetStudentByMailAsync(string studentMail)
        {
            return await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentMail == studentMail);
        }

        public async Task<bool> AddStudentAsync(Student student)
        {
            try
            {
                await _context.Students.AddAsync(student);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddClubAsync(Club club)
        {
            try
            {
                await _context.Clubs.AddAsync(club);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
