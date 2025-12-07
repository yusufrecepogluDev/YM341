using ClupApi.Models;

namespace ClupApi.Repositories.Interfaces
{
    public interface IStudentRepository : IBaseRepository<Student>
    {
        Task<Student?> GetByStudentNumberAsync(long studentNumber);
        Task<Student?> GetByEmailAsync(string email);
        Task<IEnumerable<Student>> GetActiveStudentsAsync();
    }
}
