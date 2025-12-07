using ClupApi.Models;

namespace ClupApi.Repositories.Interfaces
{
    public interface IClubRepository : IBaseRepository<Club>
    {
        Task<Club?> GetByClubNumberAsync(long clubNumber);
        Task<IEnumerable<Club>> GetActiveClubsAsync();
    }
}
